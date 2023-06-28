﻿using fbognini.Core.Data.Pagination;
using fbognini.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Core.Data
{

    public abstract class BaseSelectArgs : IArgs
    {
        public bool ThrowExceptionIfNull { get; set; } = false;

    }

    public class InMemorySelectArgs<TEntity> : BaseSelectArgs
    {
    }

    public class InMemorySelectCriteria<TEntity> : BaseSelectArgs, IHasFilter<TEntity>, IHasSearch<TEntity>, IHasSorting, IHasOffset
    {
        public InMemorySelectCriteria()
        {
        }

        public int? PageNumber { get; protected set; }
        public int? PageSize { get; protected set; }

        internal int? Total { get; set; }

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> QueryProcessing { get; set; }

        public List<KeyValuePair<string, SortingDirection>> Sorting { get; } = new List<KeyValuePair<string, SortingDirection>>();
        public Search<TEntity> Search { get; } = new Search<TEntity>();

        protected LogicalOperator Operator { get; set; } = LogicalOperator.AND;

        public virtual List<Expression<Func<TEntity, bool>>> ToWhereClause() => new List<Expression<Func<TEntity, bool>>>();

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

        public void LoadPaginationOffsetQuery(PaginationOffsetQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            PageNumber = query.PageNumber;
            PageSize = query.PageSize;
        }

        public void SetOperator(LogicalOperator logicalOperator)
        {
            Operator = logicalOperator;
        }
    }
}
