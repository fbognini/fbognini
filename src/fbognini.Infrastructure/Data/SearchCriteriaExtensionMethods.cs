
using fbognini.Core.Data.Pagination;
using fbognini.Core.Entities;
using fbognini.Core.Extensions;
using LinqKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace fbognini.Core.Data
{
    public static class SearchCriteriaExtensionMethods
    {
        public static IQueryable<T> QuerySearch<T>(this IQueryable<T> query, SearchCriteria<T> criteria, out PaginationResult pagination)
            where T : class, IAuditableEntity
            => query.QueryPagination(criteria, out pagination);

        public static string GetArgsKey<TEntity>(this IArgs args)
        {
            var builder = new StringBuilder();
            builder.Append(typeof(TEntity).Name);

            if (args is IHasViews<TEntity> hasViews)
            {
                builder.Append($"|v:");
                builder.Append(string.Join(',', hasViews.AllIncludes.OrderBy(x => x)));
            }

            if (args is IHasFilter<TEntity> hasFilter)
            {
                var filter = hasFilter.ResolveFilter();
                var key = filter.GenerateHumanReadableKey();
                builder.Append($"|f:{key}");
            }

            if (args is IHasSearch<TEntity> hasSearch)
            {
                if (hasSearch.Search != null && hasSearch.Search.AllFields.Any())
                {
                    builder.Append($"|q:");
                    builder.Append($"q={hasSearch.Search.Keyword}&t=");
                    builder.Append(string.Join(',', hasSearch.Search.AllFields.OrderBy(x => x)));
                }
            }

            if (args is IHasSorting hasSorting && hasSorting.Sorting.Any())
            {
                builder.Append($"|s:");
                builder.Append(string.Join(',', hasSorting.Sorting.OrderBy(x => x.Key).Select(s => $"{s.Key}x{s.Value}")));
            }

            if (args is IHasOffset hasOffset && hasOffset.PageSize.HasValue)
            {
                builder.Append($"|p:");
                builder.Append($"n={hasOffset.PageNumber}&s={hasOffset.PageSize}");

                if (args is IHasSinceOffset hasSinceOffset && hasSinceOffset.Since.HasValue)
                {
                    builder.Append($"&since={hasSinceOffset.Since}&after={hasSinceOffset.AfterId}");
                }
            }

            return builder.ToString();
        }
    }
}
