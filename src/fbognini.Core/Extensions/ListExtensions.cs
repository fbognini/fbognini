using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace fbognini.Core.Extensions
{
    public static class ListExtensions
    {
        public static Collection<TOut> ToCollection<T, TOut>(this List<T> items)
        {
            Collection<TOut> collection = new Collection<TOut>();

            for (int i = 0; i < items.Count; i++)
            {
                collection.Add((TOut)Convert.ChangeType(items[i], typeof(TOut)));
            }

            return collection;
        }

        public static Collection<T> ToCollection<T>(this List<T> items)
        {
            return items.ToCollection<T, T>();
        }


        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int blockSize)
        {
            while (source.Any())
            {
                yield return source.Take(blockSize);
                source = source.Skip(blockSize);
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(this List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }
}
