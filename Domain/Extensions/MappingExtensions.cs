using System.Reflection;

namespace Domain.Extensions;

public static class MappingExtensions
{
    public static TDestination MapTo<TDestination>(this object source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        TDestination destination = Activator.CreateInstance<TDestination>()!;

        var sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destinationProperties = destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var destinationProperty in destinationProperties)
        {
            var sourceProperty = sourceProperties.FirstOrDefault(x =>
                x.Name == destinationProperty.Name &&
                x.PropertyType == destinationProperty.PropertyType);

            if (sourceProperty != null && destinationProperty.CanWrite)
            {
                var value = sourceProperty.GetValue(source);
                destinationProperty.SetValue(destination, value);
            }
            else
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
        }

        return destination;
    }

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
}