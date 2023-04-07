using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace fbognini.Core.Extensions
{
    public static class EnumExtensions
    {

        public static string GetDescription(this Enum GenericEnum)
        {
            Type genericEnumType = GenericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Count() > 0))
                {
                    return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return GenericEnum.ToString();
        }

        public static List<TEnum> GetEnumList<TEnum>() where TEnum : Enum
            => ((TEnum[])Enum.GetValues(typeof(TEnum))).ToList();

        public static Dictionary<int, string> GetEnumDictionary<TEnum>() where TEnum : Enum
        {
            var type = typeof(TEnum);
            var values = Enum.GetValues(type).Cast<TEnum>();
            return values.ToDictionary(e => Convert.ToInt32(e), e => e.ToString());
        }
    }
}
