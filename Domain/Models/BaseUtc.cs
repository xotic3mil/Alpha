using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public abstract class BaseUtc
    {
        public virtual void NormalizeTimeProperties()
        {

            var properties = GetType().GetProperties()
               .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    var value = (DateTime)property.GetValue(this);
                    if (value.Kind != DateTimeKind.Utc)
                    {
                        property.SetValue(this, DateTime.SpecifyKind(value, DateTimeKind.Utc));
                    }
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    var value = (DateTime?)property.GetValue(this);
                    if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
                    {
                        property.SetValue(this, DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
                    }
                }
            }

        }
    }
}
