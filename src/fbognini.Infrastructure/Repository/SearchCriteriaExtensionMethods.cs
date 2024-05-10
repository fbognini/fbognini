using fbognini.Core.Domain.Query;
using fbognini.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace fbognini.Infrastructure.Repository
{
    public static class SearchCriteriaExtensionMethods
    {
        public static IQueryable<T> QueryArgs<T>(this IQueryable<T> query, SelectArgs<T>? criteria)
            where T : class
        {
            criteria ??= new SelectArgs<T>();

            if (!criteria.Track)
            {
                query = query.AsNoTracking();
            }

            if (criteria.IgnoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (criteria.IgnoreAutoIncludes)
            {
                query = query.IgnoreAutoIncludes();
            }

            query = query.IncludeViews(criteria);

            return query;
        }

        public static IQueryable<T> IncludeViews<T>(this IQueryable<T> list, IHasViews<T> includes)
            where T : class
        {
            if (includes == null)
            {
                return list;
            }

            foreach (var view in includes.Includes)
            {
                var path = view.GetPropertyPath(true); // it must works even with x.OrderLines.First().Product.Retailer.DisplayName
                list = list.Include(path);
            }

            foreach (var view in includes.IncludeStrings)
            {
                list = list.Include(view);
            }

            return list;
        }

        public static IQueryable<T> IncludeViews<T>(this IQueryable<T> list, List<Expression<Func<T, object>>> includes)
            where T : class
        {
            if (includes == null)
            {
                return list;
            }

            foreach (var view in includes)
            {
                list = list.Include(view);
            }

            return list;
        }

        


           }
}
