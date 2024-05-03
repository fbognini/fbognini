using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Core.Domain.Query;

public interface IHasSorting<T>: IHasSorting
{
    void AddSorting(string criteria, SortingDirection direction);
    void AddSorting(Expression<Func<T, object?>> criteria, SortingDirection direction);
    void ClearSorting();
}

public interface IHasSorting
{
    IReadOnlyList<KeyValuePair<string, SortingDirection>> Sorting { get; }
    void LoadSortingQuery(SortingQuery query);
}

public static class HasSortingExtensionMethods
{
    public static string GetArgsKey<T>(this IHasSorting<T> filter)
    {
        if (filter.Sorting.Count == 0)
        {
            return string.Empty;
        }

        var sorts = string.Join(',', filter.Sorting.OrderBy(x => x.Key).Select(s => $"{s.Key}x{s.Value}"));

        return $"|s:{sorts}";
    }

    public static List<KeyValuePair<string, object?>> GetArgsKeyAsDictionary<T>(this IHasSorting<T> filter)
    {
        if (filter.Sorting.Count == 0)
        {
            return new();
        }

        var sorts = filter.Sorting.OrderBy(x => x.Key)
            .Select(x => new KeyValuePair<string, object?>(x.Key, x.Value))
            .ToDictionary(x => x.Key, x => x.Value);

        return new List<KeyValuePair<string, object?>>()
        {
            new(nameof(IHasSorting<T>.Sorting), sorts)
        };
    }
}
