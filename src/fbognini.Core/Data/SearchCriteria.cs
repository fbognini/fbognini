using fbognini.Core.Data.Pagination;
using fbognini.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace fbognini.Core.Data
{
    public interface IHasSearch<TEntity>
    {
        Search<TEntity> Search { get; }
    }

    public interface IHasSorting
    {
        List<KeyValuePair<string, SortingDirection>> Sorting { get; }
        void LoadSortingQuery(SortingQuery query);
    }

    public interface IHasViews<TEntity>
    {
        List<Expression<Func<TEntity, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
    }

    public class SelectArgs<TEntity> : IHasViews<TEntity>
        where TEntity : IEntity
    {
        public bool Track { get; set; } = true;
        public List<Expression<Func<TEntity, object>>> Includes { get; } = new List<Expression<Func<TEntity, object>>>();
        public List<string> IncludeStrings { get; } = new List<string>();
    }

    public abstract class SelectCriteria<TEntity> : SelectArgs<TEntity>, IHasSearch<TEntity>, IHasSorting
        where TEntity : IEntity
    {
        protected LogicalOperator Operator { get; set; } = LogicalOperator.AND;

        public bool Track { get; set; } = false;
        public List<KeyValuePair<string, SortingDirection>> Sorting { get; } = new List<KeyValuePair<string, SortingDirection>>();
        public Search<TEntity> Search { get; } = new Search<TEntity>();

        public void SetOperator(
            LogicalOperator logicalOperator)
        {
            Operator = logicalOperator;
        }

        public abstract List<Expression<Func<TEntity, bool>>> ToWhereClause();

        public Expression<Func<TEntity, bool>> ResolveFilter()
        {
            var list = ToWhereClause();
            return list.BuildPredicate(Operator);
        }

        public void LoadSortingQuery(SortingQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            Sorting.Add(new KeyValuePair<string, SortingDirection>(query.SortingCriteria, query.SortingDirection));
        }
    }

    public abstract class SearchCriteria<TEntity> : SelectCriteria<TEntity>
        where TEntity : IAuditableEntity
    {
        public int? PageNumber { get; private set; }
        public int? PageSize { get; private set; }
        internal long? Since { get; private set; }
        internal int? AfterId { get; private set; }

        internal int? Total { get; set; }
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

        public void LoadPaginationOffsetQuery(PaginationOffsetQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            PageNumber = query.PageNumber;
            PageSize = query.PageSize;
        }

    }
}
