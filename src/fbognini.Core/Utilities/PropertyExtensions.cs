using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace fbognini.Core.Utilities
{
    public static class PropertyExtensions
    {
        public static PropertyInfo GetNestedProperty(this Type type, string propertyName, BindingFlags bindingFlags = BindingFlags.Default)
        {
            return type.GetNestedProperty(propertyName.Split('.'), bindingFlags);
        }

        public static PropertyInfo GetNestedProperty(this Type type, IEnumerable<string> propertyNames, BindingFlags bindingFlags = BindingFlags.Default)
        {
            var property = type.GetProperty(propertyNames.First(), bindingFlags);
            propertyNames = propertyNames.Skip(1);
            if (propertyNames.Any())
            {
                return GetNestedProperty(property.GetType(), propertyNames, bindingFlags);
            }

            return property;
        }
    }
}
