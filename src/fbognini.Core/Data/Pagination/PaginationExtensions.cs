using fbognini.Core.Entities;
using System;
using System.Linq;

namespace fbognini.Core.Data.Pagination
{
    public static class PaginationExtensions
    {
        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, SelectCriteria<T> offsetCriteria)
        {
            return list.QueryPagination(offsetCriteria, out var _);
        }

        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, SearchCriteria<T> searchCriteria)
            where T : AuditableEntity
        {
            return list.QueryPagination(searchCriteria, out var _);
        }

        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, SelectCriteria<T> offsetCriteria, out Pagination pagination)
        {
            if (!offsetCriteria.PageSize.HasValue)
            {
                pagination = null;
                return list;
            }

            pagination = new Pagination();

            int total = list.Count();
            pagination.Total = total;
            offsetCriteria.Total = total;

            if (offsetCriteria.PageSize == -1)
            {
                return list;
            }

            pagination.PageSize = offsetCriteria.PageSize.Value;
            if (offsetCriteria.PageNumber.HasValue)
            {

                list = list
                    .Skip((offsetCriteria.PageNumber.Value - 1) * offsetCriteria.PageSize.Value)
                    .Take(offsetCriteria.PageSize.Value);
                pagination.PageNumber = offsetCriteria.PageNumber.Value;
            }

            return list;
        }


        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, SearchCriteria<T> searchCriteria, out Pagination pagination)
            where T : AuditableEntity
        {
            if (!searchCriteria.PageSize.HasValue)
            {
                pagination = null;
                return list;
            }

            pagination = new Pagination();

            int total = list.Count();
            pagination.Total = total;
            searchCriteria.Total = total;

            if (searchCriteria.PageSize == -1)
            {
                return list;
            }

            pagination.PageSize = searchCriteria.PageSize.Value;
            if (searchCriteria.PageNumber.HasValue)
            {
                list = list
                    .Skip((searchCriteria.PageNumber.Value - 1) * searchCriteria.PageSize.Value)
                    .Take(searchCriteria.PageSize.Value);
                pagination.PageNumber = searchCriteria.PageNumber.Value;
            }
            else if (searchCriteria.Since.HasValue)
            {
                var since = new DateTime(1970, 1, 1).AddTicks(searchCriteria.Since.Value * 10000);
                list = list.Where(x => x.LastUpdated >= since);

                if (list is IQueryable<AuditableEntityWithIdentity> ilist)
                {
                    list = ilist.Where(x => searchCriteria.AfterId == null || x.Id > searchCriteria.AfterId).Cast<T>();
                }

                pagination.PartialTotal = list.Count();

                list = list
                    .OrderBy(x => x.LastUpdated)
                    .Take(searchCriteria.PageSize.Value);

                var last = list.LastOrDefault();
                if (last != null)
                {
                    long continuationSince = Convert.ToInt64(last.LastUpdated.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds);
                    string continuation = continuationSince.ToString();

                    if (last is AuditableEntityWithIdentity ilast)
                        continuation = continuation + "_" + ilast.Id.ToString();

                    pagination.ContinuationSince = continuation;
                    searchCriteria.ContinuationSince = continuation;
                }
                else
                {
                    pagination.ContinuationSince = searchCriteria.Since.ToString();
                    if (searchCriteria.AfterId != null)
                        pagination.ContinuationSince += "_" + searchCriteria.AfterId;
                }
            }

            return list;
        }
    }
}
