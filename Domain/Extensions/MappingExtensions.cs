using System.Collections;
using System.Reflection;

namespace Domain.Extensions;

public static class MappingExtensions
{
    // ThreadLocal stack to track objects being mapped to detect circular references
    private static readonly ThreadLocal<Stack<object>> _mappingStack = new(() => new Stack<object>());

    /// <summary>
    /// Maps an object to a new object of type TDestination
    /// with protection against circular references
    /// </summary>
    /// <typeparam name="TDestination">The destination type to create</typeparam>
    /// <param name="source">The source object to map from</param>
    /// <returns>A new instance of TDestination with properties copied from source</returns>
    public static TDestination MapTo<TDestination>(this object source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        // Initialize the stack if this is the first call
        var stack = _mappingStack.Value!;

        // Check for circular references
        foreach (var item in stack)
        {
            if (ReferenceEquals(source, item))
            {
                // Return default if circular reference detected
                return Activator.CreateInstance<TDestination>()!;
            }
        }

        // Push current object to stack
        stack.Push(source);

        try
        {
            TDestination destination = Activator.CreateInstance<TDestination>()!;

            var sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var destinationProperty in destinationProperties)
            {
                var sourceProperty = sourceProperties.FirstOrDefault(x =>
                    x.Name == destinationProperty.Name &&
                    (x.PropertyType == destinationProperty.PropertyType ||
                     (IsCollectionType(x.PropertyType) && IsCollectionType(destinationProperty.PropertyType))));

                if (sourceProperty != null && destinationProperty.CanWrite)
                {
                    var value = sourceProperty.GetValue(source);

                    // Handle collection properties
                    if (value != null && IsCollectionType(sourceProperty.PropertyType) && IsCollectionType(destinationProperty.PropertyType))
                    {
                        MapCollection(value, destinationProperty, destination);
                        continue;
                    }

                    destinationProperty.SetValue(destination, value);
                }
                else
                {
                    MapNavigationProperty(source, sourceProperties, destinationProperty, destination);
                }
            }
            return destination;
        }
        finally
        {
            // Pop the object from stack when done (even if an exception occurs)
            stack.Pop();
        }
    }

    /// <summary>
    /// Maps a collection property from source to destination
    /// </summary>
    private static void MapCollection(object value, PropertyInfo destinationProperty, object destination)
    {
        // Create appropriate collection instance based on destination property type
        var collection = CreateCollectionInstance(destinationProperty.PropertyType);

        // Get the element types
        var sourceElementType = GetElementType(value.GetType());
        var destElementType = GetElementType(destinationProperty.PropertyType);

        if (sourceElementType != null && destElementType != null && collection != null)
        {
            // If element types match or are both mappable complex types
            var addMethod = collection.GetType().GetMethod("Add");

            if (addMethod != null)
            {
                foreach (var item in (IEnumerable)value)
                {
                    // Skip null items
                    if (item == null) continue;

                    // If types match directly, add the item
                    if (item.GetType() == destElementType || destElementType.IsAssignableFrom(item.GetType()))
                    {
                        addMethod.Invoke(collection, new[] { item });
                    }
                    // If types don't match but we can map them
                    else if (!IsSimpleType(item.GetType()) && !IsSimpleType(destElementType))
                    {
                        var mapToMethod = typeof(MappingExtensions)
                            .GetMethod(nameof(MapTo), BindingFlags.Public | BindingFlags.Static)
                            ?.MakeGenericMethod(destElementType);

                        if (mapToMethod != null)
                        {
                            var mappedItem = mapToMethod.Invoke(null, new[] { item });
                            addMethod.Invoke(collection, new[] { mappedItem });
                        }
                    }
                }
            }
        }
        destinationProperty.SetValue(destination, collection);
    }

    /// <summary>
    /// Maps a navigation property from source to destination
    /// </summary>
    private static void MapNavigationProperty(object source, PropertyInfo[] sourceProperties,
        PropertyInfo destinationProperty, object destination)
    {
        var potentialNavProperty = sourceProperties.FirstOrDefault(x =>
            x.Name == destinationProperty.Name &&
            x.PropertyType != destinationProperty.PropertyType &&
            !IsSimpleType(x.PropertyType) &&
            !IsSimpleType(destinationProperty.PropertyType));

        if (potentialNavProperty != null && destinationProperty.CanWrite)
        {
            var navValue = potentialNavProperty.GetValue(source);

            if (navValue != null)
            {
                var destType = destinationProperty.PropertyType;
                var mapToMethod = typeof(MappingExtensions)
                    .GetMethod(nameof(MapTo), BindingFlags.Public | BindingFlags.Static)
                    ?.MakeGenericMethod(destType);

                if (mapToMethod != null)
                {
                    var mappedNav = mapToMethod.Invoke(null, new[] { navValue });
                    destinationProperty.SetValue(destination, mappedNav);
                }
            }
        }
    }

    /// <summary>
    /// Determines if a type is a simple value type or string
    /// </summary>
    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateOnly) ||
               type == typeof(TimeOnly) ||
               type == typeof(Guid) ||
               Nullable.GetUnderlyingType(type) != null;
    }

    /// <summary>
    /// Determines if a type is a collection type
    /// </summary>
    private static bool IsCollectionType(Type type)
    {
        // Check if it's a collection type (excluding string which is IEnumerable<char>)
        return type != typeof(string) &&
               (typeof(IEnumerable).IsAssignableFrom(type) ||
                (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type.GetGenericTypeDefinition())));
    }

    /// <summary>
    /// Gets the element type of a collection type
    /// </summary>
    private static Type? GetElementType(Type collectionType)
    {
        // Handle arrays
        if (collectionType.IsArray)
            return collectionType.GetElementType();

        // Handle generic collections
        if (collectionType.IsGenericType)
        {
            var args = collectionType.GetGenericArguments();
            if (args.Length > 0)
                return args[0];
        }

        // Try to find element type through interfaces
        foreach (var interfaceType in collectionType.GetInterfaces())
        {
            if (interfaceType.IsGenericType &&
                (interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 interfaceType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                 interfaceType.GetGenericTypeDefinition() == typeof(IList<>)))
            {
                return interfaceType.GetGenericArguments()[0];
            }
        }

        return null;
    }

    /// <summary>
    /// Creates an appropriate collection instance for the given collection type
    /// </summary>
    private static object? CreateCollectionInstance(Type collectionType)
    {
        if (collectionType.IsInterface)
        {
            if (collectionType.IsGenericType)
            {
                var genericType = collectionType.GetGenericTypeDefinition();
                var elementType = collectionType.GetGenericArguments()[0];

                // Map common interfaces to concrete implementations
                if (genericType == typeof(ICollection<>) ||
                    genericType == typeof(IList<>) ||
                    genericType == typeof(IEnumerable<>))
                {
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    return Activator.CreateInstance(listType);
                }
                else if (genericType == typeof(ISet<>))
                {
                    var hashSetType = typeof(HashSet<>).MakeGenericType(elementType);
                    return Activator.CreateInstance(hashSetType);
                }
            }
            return null; // Unsupported interface type
        }

        // For concrete classes that have a parameterless constructor
        if (!collectionType.IsAbstract && collectionType.GetConstructor(Type.EmptyTypes) != null)
        {
            return Activator.CreateInstance(collectionType);
        }

        // For arrays
        if (collectionType.IsArray)
        {
            var elementType = collectionType.GetElementType();
            if (elementType != null)
                return Array.CreateInstance(elementType, 0);
        }

        return null;
    }
}