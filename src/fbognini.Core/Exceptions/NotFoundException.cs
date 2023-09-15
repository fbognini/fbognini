using fbognini.Core.Data;
using fbognini.Core.Extensions;
using fbognini.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace fbognini.Core.Exceptions
{
    public class NotFoundAdditionalData : AdditionalData
    {
        public override string Entity { get; }
        public object Key { get; set; }

        public NotFoundAdditionalData(string entity, object key)
            : base("NotFound")
        {
            Entity = entity;
            Key = key;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException()
            : base(HttpStatusCode.NotFound)
        {

        }

        public NotFoundException(
            string message, string? title = null)
            : base(HttpStatusCode.NotFound, message, title)
        {

        }

        public NotFoundException(
            Type type,
            object key)
            : base(
                  HttpStatusCode.NotFound,
                  $"Entity \"{type.Name}\" ({SerializedKey(key)}) was not found.",
                  $"{type.Name} was not found.",
                  new NotFoundAdditionalData(type.Name, FormatKey(key)))
        {
        }

        private static string SerializedKey(object key)
        {
            key = FormatKey(key);

            if (key is string || key.GetType().IsValueType)
            {
                return key.ToString();
            }

            if (key is Dictionary<string, object> dict)
            {
                return ToDebugString(dict);
            }

            return ToDebugString(key.ToDictionary());
        }

        private static object FormatKey(object key)
        {

            if (key is IArgs)
            {
                var names = key.GetType()
                    .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    .Select(pi => pi.Name).ToList();

                var newKey = new Dictionary<string, object>();
                foreach (var name in names)
                {
                    newKey.Add(name, key.GetPropertyValue(name));
                }

                return newKey;
            }

            return key;
        }

        private static string ToDebugString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            return "{ " + string.Join(", ", dictionary.Select(kv => kv.Key + " = " + kv.Value).ToArray()) + " }";
        }
    }
}
