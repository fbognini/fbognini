using fbognini.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Core.Domain.Query;

public interface IHasViews<TEntity>
{
    List<string> AllIncludes
    {
        get
        {
            var allViews = Includes.Select(x => x.GetPropertyPath(true)).ToList();
            allViews.AddRange(IncludeStrings);

            return allViews;
        }
    }

    List<Expression<Func<TEntity, object?>>> Includes { get; }
    List<string> IncludeStrings { get; }
}
public static class HasViewsExtensionMethods
{
    public static string GetArgsKey<T>(this IHasViews<T> filter)
    {
        if (filter.AllIncludes.Count == 0)
        {
            return string.Empty;
        }

        var views = string.Join(',', filter.AllIncludes.OrderBy(x => x));

        return $"|v:{views}";
    }

    public static List<KeyValuePair<string, object?>> GetArgsKeyAsDictionary<T>(this IHasViews<T> filter)
    {
        if (filter.AllIncludes.Count == 0)
        {
            return new();
        }

        return new List<KeyValuePair<string, object?>>()
        {
            new("Includes", filter.AllIncludes.OrderBy(x => x))
        };
    }
}
