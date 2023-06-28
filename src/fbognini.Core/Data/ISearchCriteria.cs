using fbognini.Core.Entities;
using fbognini.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace fbognini.Core.Data
{
    public interface IArgs
    {
    }

    public interface IHasFilter<TEntity>
    {
        Expression<Func<TEntity, bool>> ResolveFilter();
    }

    public interface IHasSearch<TEntity>
    {
        Search<TEntity> Search { get; }
    }

    public interface IHasSorting
    {
        List<KeyValuePair<string, SortingDirection>> Sorting { get; }
        void LoadSortingQuery(SortingQuery query);
    }

    public interface IHasOffset
    {
        int? PageNumber { get; }
        int? PageSize { get; }
    }
}
