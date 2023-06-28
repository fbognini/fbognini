using fbognini.Core.Data.Pagination;
using fbognini.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Core.Data
{

    public class SelectArgs<TEntity> : InMemorySelectArgs<TEntity>, IRepositoryArgs<TEntity>
    {
        public bool Track { get; set; } = true;
        public List<Expression<Func<TEntity, object>>> Includes { get; } = new List<Expression<Func<TEntity, object>>>();
        public List<string> IncludeStrings { get; } = new List<string>();
    }

    public class SelectCriteria<TEntity> : InMemorySelectCriteria<TEntity>, IRepositoryArgs<TEntity>
    {
        public SelectCriteria()
        {
        }

        public bool Track { get; set; } = true;
        public List<Expression<Func<TEntity, object>>> Includes { get; } = new List<Expression<Func<TEntity, object>>>();
        public List<string> IncludeStrings { get; } = new List<string>();

        internal int? Total { get; set; }
    }

    public class SearchCriteria<TEntity> : SelectCriteria<TEntity>, IHasSinceOffset
        where TEntity : IAuditableEntity
    {
        public long? Since { get; private set; }
        public int? AfterId { get; private set; }

        internal string ContinuationSince { get; set; }

        public void LoadPaginationAdvancedSinceQuery(PaginationAdvancedSinceQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            PageSize = query.PageSize;

            if (query.Since != null)
            {
                var since = query.Since.Split("_");
                Since = long.Parse(since[0]);
                AfterId = since.Length > 1 ? int.Parse(since[1]) : default(int?);
            }
            else
            {
                Since = 0;
                AfterId = null;
            }
        }

        public void LoadPaginationSinceQuery(PaginationSinceQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            PageSize = query.PageSize;
            Since = query.Since ?? 0;
        }
    }
}
