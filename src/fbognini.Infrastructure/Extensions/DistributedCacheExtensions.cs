using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

            var value = JsonSerializer.Deserialize<T>(bytes, GetJsonSerializerOptions());
            return value;
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, CancellationToken cancellationToken = default)
        {
            return SetAsync(cache, key, value, new DistributedCacheEntryOptions(), cancellationToken);
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, GetJsonSerializerOptions()));
            return cache.SetAsync(key, bytes, options, cancellationToken);
        }

        public static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
        {
            var val = cache.Get(key);
            value = default;
            if (val == null || val.Length == 0)
            {
                return false;
            }
            value = JsonSerializer.Deserialize<T>(val, GetJsonSerializerOptions());
            return true;
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions()
            {
                PropertyNamingPolicy = null,
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }
    }
}
