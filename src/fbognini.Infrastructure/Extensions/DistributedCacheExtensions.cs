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

        public static Task<T?> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<CancellationToken, Task<T>> get, CancellationToken cancellationToken = default)
        {
            return cache.GetOrSetAsync(key, get, new DistributedCacheEntryOptions(), cancellationToken);
        }

        public static async Task<T?> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<CancellationToken, Task<T>> get, DistributedCacheEntryOptions distributedCacheEntryOptions, CancellationToken cancellationToken = default)
        {
            if (!cache.TryGetValue<T>(key, out var entity))
            {
                entity = await get(cancellationToken);
                if (entity == null)
                {
                    return default;
                }

                await cache.SetAsync(key, entity, distributedCacheEntryOptions, cancellationToken: cancellationToken);
            }

            return entity;
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions()
            {
                PropertyNamingPolicy = null,
                WriteIndented = false,
                AllowTrailingCommas = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }
    }
}
