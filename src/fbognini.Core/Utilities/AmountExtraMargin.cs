using fbognini.Core.Extensions;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

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

                    foreach (var item in (propertyValue as IEnumerable)!)
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

        public static double ConvertAmount(object obj, AmountConversionRate rate)
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

        public override string ToString()
        {
            return $"{Ratio.ToString(CultureInfo.InvariantCulture)},{ExtraMarginPercentage.ToString(CultureInfo.InvariantCulture)},{ExtraMarginValue.ToString(CultureInfo.InvariantCulture)}";
        }

        public static bool TryParse(string? value, out AmountConversionRate? currencyConversion)
        {
            var currency = CultureInfo.InvariantCulture;

            try
            {
                if (value == null)
                {
                    currencyConversion = null;
                    return false;
                }

                var splitValue = value.Split(',').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                if (splitValue.Length != 3)
                {
                    currencyConversion = null;
                    return false;
                }

                currencyConversion = new AmountConversionRate()
                {
                    Ratio = splitValue[0],
                    ExtraMarginPercentage = splitValue[1],
                    ExtraMarginValue = splitValue[2]
                };

                return true;
            }
            catch (Exception)
            {
                currencyConversion = null;
                return false;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AmountAttribute : Attribute
    {
    }


}
