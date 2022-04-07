using fbognini.Core.Utilities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Core.Data
{
    public static class SearchCriteriaExtensionMethods
    {
        public static IQueryable<T> IncludeViews<T, TViews>(this IQueryable<T> list, IList<TViews> views)
            where T : class
            where TViews : struct, IConvertible
        {
            if (views == null)
                return list;

            foreach (var view in views)
            {
                list = list.Include(view.ToString().Replace('_', '.'));
            }

            return list;
        }

        public static IQueryable<T> IncludeViews<T>(this IQueryable<T> list, IHasViews<T> includes)
            where T : class
        {
            if (includes == null)
                return list;

            foreach (var view in includes.Includes)
            {
                var path = Utils.GetPropertyPath(view, true); // it must works even with x.OrderLines.First().Product.Retailer.DisplayName
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
                return list;

            foreach (var view in includes)
            {
                list = list.Include(view);
            }

            return list;
        }

    }
}
