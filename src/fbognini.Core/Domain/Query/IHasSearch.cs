using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fbognini.Core.Domain.Query;

public interface IHasSearch<TEntity>
{
    Search<TEntity> Search { get; }
}
public static class HasSearchExtensionMethods
{
    public static string GetArgsKey<T>(this IHasSearch<T> filter)
    {
        if (filter.Search == null || filter.Search.AllFields.Count == 0)
        {
            return string.Empty;
        }

        var keyword = filter.Search.Keyword;
        var fields = string.Join(',', filter.Search.AllFields.OrderBy(x => x));

        return $"|q:q={filter.Search.Keyword}&t={fields}";
    }

    public static List<KeyValuePair<string, object?>> GetArgsKeyAsDictionary<T>(this IHasSearch<T> filter)
    {
        if (filter.Search == null || filter.Search.AllFields.Count == 0)
        {
            return new();
        }

        var pairs = new Dictionary<string, object?>
        {
            [nameof(Search<T>.Keyword)] = filter.Search.Keyword,
            ["Fields"] = filter.Search.AllFields.OrderBy(x => x)
        };

        return new List<KeyValuePair<string, object?>>()
        {
            new(nameof(IHasSearch<T>.Search), pairs)
        };
    }
}