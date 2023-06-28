using fbognini.Core.Entities;
using System;
using System.Linq;

namespace fbognini.Core.Data.Pagination
{
    public static class PaginationExtensions
    {
        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, InMemorySelectCriteria<T> offsetCriteria)
        {
            return list.QueryPagination(offsetCriteria, out var _);
        }

        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, InMemorySelectCriteria<T> selectCriteria, out PaginationResult pagination)
        {
            if (!selectCriteria.PageSize.HasValue)
            {
                pagination = null;
                return list;
            }

            pagination = new PaginationResult();

            int total = list.Count();
            pagination.Total = total;
            selectCriteria.Total = total;

            if (selectCriteria.PageSize == -1)
            {
                return list;
            }

            pagination.PageSize = selectCriteria.PageSize.Value;
            if (selectCriteria.PageNumber.HasValue)
            {
                list = list
                    .Skip((selectCriteria.PageNumber.Value - 1) * selectCriteria.PageSize.Value)
                    .Take(selectCriteria.PageSize.Value);
                pagination.PageNumber = selectCriteria.PageNumber.Value;
            }

            return list;
        }

    }
}
