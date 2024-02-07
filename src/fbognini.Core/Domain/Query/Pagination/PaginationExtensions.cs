using fbognini.Core.Domain.Query;
using System;
using System.Linq;

namespace fbognini.Core.Domain.Query.Pagination
{
    public static class PaginationExtensions
    {
        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, QueryableCriteria<T> offsetCriteria)
        {
            return list.QueryPagination(offsetCriteria, out var _);
        }

        public static IQueryable<T> QueryPagination<T, TKey>(this IQueryable<T> list, QueryableAuditableCriteria<T> searchCriteria)
            where T : IHaveId<long>, IHaveLastUpdated
        {
            return list.QueryPagination(searchCriteria, out var _);
        }

        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, QueryableCriteria<T> selectCriteria, out PaginationResult? pagination)
        {
            if (!selectCriteria.Page.Size.HasValue)
            {
                pagination = null;
                return list;
            }

            pagination = new PaginationResult();

            int total = list.Count();
            pagination.Total = total;
            selectCriteria.Page.Total = total;

            if (selectCriteria.Page.Size == -1)
            {
                return list;
            }

            pagination.PageSize = selectCriteria.Page.Size.Value;
            if (selectCriteria.Page.Number.HasValue)
            {
                list = list
                    .Skip((selectCriteria.Page.Number.Value - 1) * selectCriteria.Page.Size.Value)
                    .Take(selectCriteria.Page.Size.Value);
                pagination.PageNumber = selectCriteria.Page.Number.Value;
            }

            return list;
        }

        public static IQueryable<T> QueryPagination<T>(this IQueryable<T> list, QueryableAuditableCriteria<T> searchCriteria, out PaginationResult? pagination)
            where T : IHaveId<long>, IHaveLastUpdated
        {
            if (!searchCriteria.Page.Size.HasValue)
            {
                pagination = null;
                return list;
            }

            pagination = new PaginationResult();

            int total = list.Count();
            pagination.Total = total;
            searchCriteria.Page.Total = total;

            if (searchCriteria.Page.Size == -1)
            {
                return list;
            }

            pagination.PageSize = searchCriteria.Page.Size.Value;
            if (searchCriteria.Page.Number.HasValue)
            {
                list = list
                    .Skip((searchCriteria.Page.Number.Value - 1) * searchCriteria.Page.Size.Value)
                    .Take(searchCriteria.Page.Size.Value);
                pagination.PageNumber = searchCriteria.Page.Number.Value;
            }
            else if (searchCriteria.Page.Since.HasValue)
            {
                var since = new DateTime(1970, 1, 1).AddTicks(searchCriteria.Page.Since.Value * 10000);
                list = list.Where(x => x.LastUpdated >= since);

                if (searchCriteria.Page.AfterId is not null)
                {
                    list = list.Where(x => x.Id > searchCriteria.Page.AfterId.Value);
                }

                pagination.PartialTotal = list.Count();

                list = list
                    .OrderBy(x => x.LastUpdated)
                    .Take(searchCriteria.Page.Size.Value);

                var last = list.LastOrDefault();
                if (last != null)
                {
                    long continuationSince = Convert.ToInt64(last.LastUpdated.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds);
                    string continuation = $"{continuationSince}_{last.Id}";

                    pagination.ContinuationSince = continuation;
                    searchCriteria.Page.ContinuationSince = continuation;
                }
                else
                {
                    pagination.ContinuationSince = searchCriteria.Page.Since.ToString();
                    if (searchCriteria.Page.AfterId != null)
                    {
                        pagination.ContinuationSince += "_" + searchCriteria.Page.AfterId;
                    }
                }
            }

            return list;
        }
    }
}
