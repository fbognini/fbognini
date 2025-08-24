using fbognini.Core.Domain.Query.Pagination;
using System;

namespace fbognini.Core.Domain.Query
{
    public class QueryableAuditableCriteria<T> : QueryableCriteria<T>
        where T : IHaveId<long>, IHaveLastUpdated
    {
        public override PageAuditableCriteria Page { get; } = new();

        public void LoadPaginationAdvancedSinceQuery(PaginationAdvancedSinceQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Page.Size = query.PageSize;

            if (query.Since != null)
            {
                var since = query.Since.Split("_");
                Page.Since = long.Parse(since[0]);
                Page.AfterId = since.Length > 1
                    ? long.Parse(since[1])
                    : null;
            }
            else
            {
                Page.Since = 0;
                Page.AfterId = null;
            }
        }

        public void LoadPaginationSinceQuery(PaginationSinceQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Page.Size = query.PageSize;
            Page.Since = query.Since ?? 0;
        }
    }
}
