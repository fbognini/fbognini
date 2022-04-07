using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Application.Utilities
{
    public static class CacheKeys
    {
        public static string GetCacheKey<T>(object id)
        {
            return $"{typeof(T).Name}-{id}";
        }

        public static string GetCacheKey(string name, object id)
        {
            return $"{name}-{id}";
        }

        public static string GetCacheKey<T>(IEnumerable<object> ids)
        {
            return GetCacheKey<T>(string.Join(';', ids));
        }
    }
}
