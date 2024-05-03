using fbognini.Core.Domain.Query;
using fbognini.Core.Extensions;
using fbognini.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;

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

    public class NotFoundException<T> : NotFoundException
    {
        public NotFoundException(QueryableCriteria<T> key)
            : base(typeof(T), key)
        {

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

        protected NotFoundException(string typeName, string serializedKey, object key)
            : base(
                  HttpStatusCode.NotFound,
                  $"Entity \"{typeName}\" ({serializedKey}) was not found.",
                  $"{typeName} was not found.",
                  new NotFoundAdditionalData(typeName, key))
        {

        }

        public NotFoundException(
            Type type,
            object key)
            : this(type.Name, SerializedKey(key), FormatKey(key))
        {
        }

        private static string SerializedKey(object key)
        {
            key = FormatKey(key);

            if (key is string || key.GetType().IsValueType)
            {
                return key.ToString() ?? string.Empty;
            }

            if (key is IDictionary<string, object?> dict)
            {
                return ToDebugString(dict);
            }

            return ToDebugString(key.ToDictionary());
        }

        private static object FormatKey(object key)
        {
            if (key is IArgs args)
            {
                return args.GetArgsKeyAsDictionary().ToDictionary(x => x.Key, x => x.Value);
            }
            return key;
        }

        private static string ToDebugString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(dictionary, jsonSerializerOptions);
        }
    }
}
