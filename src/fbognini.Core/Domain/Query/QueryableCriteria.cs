using fbognini.Core.Domain.Query.Pagination;
using fbognini.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Core.Domain.Query
{
    public class QueryableCriteria<T> : IHasFilter<T>, IHasSearch<T>, IHasSorting<T>
    {
        private readonly List<KeyValuePair<string, SortingDirection>> _sorting = new();

        public QueryableCriteria()
        {
        }

        public Func<IQueryable<T>, IQueryable<T>>? QueryProcessing { get; set; }

        public virtual PageCriteria Page { get; } = new();
        public IReadOnlyList<KeyValuePair<string, SortingDirection>> Sorting => _sorting;
        public Search<T> Search { get; } = new Search<T>();

        protected LogicalOperator Operator { get; set; } = LogicalOperator.AND;

        public virtual List<Expression<Func<T, bool>>> ToWhereClause() => new List<Expression<Func<T, bool>>>();

        public Expression<Func<T, bool>> ResolveFilter()
        {
            var list = ToWhereClause();
            return list.BuildPredicate(Operator);
        }


        public void AddSorting(string criteria, SortingDirection direction)
        {
            _sorting.Add(new KeyValuePair<string, SortingDirection>(criteria, direction));
        }

        public void AddSorting(Expression<Func<T, object?>> criteria, SortingDirection direction)
        {
            AddSorting(criteria.GetPropertyPath(true), direction);
        }

        public void LoadSortingQuery(SortingQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            AddSorting(query.SortingCriteria, query.SortingDirection);
        }

        public void ClearSorting()
        {
            _sorting.Clear();
        }

        public void LoadPaginationOffsetQuery(PaginationOffsetQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            Page.Number = query.PageNumber;
            Page.Size = query.PageSize;
        }

        public void SetOperator(LogicalOperator logicalOperator)
        {
            Operator = logicalOperator;
        }
    }

}
