using fbognini.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace fbognini.Core.Utilities
{
    public static class AmountExtraMargin
    {
        public static void ApplyConversion<T>(T value, AmountConversionRate rate)
        {
            var props = PropertyExtensions.GetPropertiesWithAttribute<AmountAttribute>(value, true);
            foreach (var (property, instance) in props)
            {
                var propertyValue = property.GetValue(instance);

                if (PropertyExtensions.IsSimpleType(property.PropertyType))
                {
                    var amount = ConvertAmount(propertyValue, rate);
                    property.SetValue(instance, Convert.ChangeType(amount, property.PropertyType));
                }
                else if (PropertyExtensions.IsEnumerableOfSimpleTypes(property.PropertyType))
                {
                    IList list = (IList)Activator.CreateInstance(property.PropertyType);

                    foreach (var item in propertyValue as IEnumerable)
                    {
                        var amount = ConvertAmount(item, rate);
                        list.Add(amount);
                    }

                    property.SetValue(instance, list, null);
                }
                else
                {
                    throw new ArgumentException($"Invalid type {property.PropertyType} for conversion");
                }
            }
        }

        private static double ConvertAmount(object obj, AmountConversionRate rate)
        {
            var amount = GetAmount(obj);

            amount *= rate.Ratio;
            amount += amount * rate.ExtraMarginPercentage / 100.0;
            amount += rate.ExtraMarginValue;
            amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

            return amount;
        }

        private static double GetAmount(object obj)
        {
            if (obj.GetType() == typeof(string) && obj.ToString() == "")
            {
                return 0;
            }

            return Convert.ToDouble(obj);
        }

        
    }

    public class AmountConversionRate
    {
        public double Ratio { get; set; } = 1;
        public double ExtraMarginPercentage { get; set; }
        public double ExtraMarginValue { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AmountAttribute : Attribute
    {
    }


}
