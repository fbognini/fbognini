using fbognini.Core.Data;
using fbognini.Core.Entities;
using fbognini.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Infrastructure.Extensions
{
    public static class SearchCriteriaExtensionMethods
    {
        public static IQueryable<T> QueryArgs<T>(this IQueryable<T> query, IRepositoryArgs<T> criteria)
            where T : class
        {
            if (criteria == null)
            {
                return query;
            }

            if (!criteria.Track)
            {
                query = query.AsNoTracking();
            }

            query = query.IncludeViews(criteria);

            return query;
        }

        private static IQueryable<T> IncludeViews<T>(this IQueryable<T> list, IHasViews<T> includes)
            where T : class
        {
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
