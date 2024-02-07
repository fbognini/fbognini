using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace fbognini.Core.Domain.Query;

public interface IHasSorting<T>: IHasSorting
{
    void AddSorting(string criteria, SortingDirection direction);
    void AddSorting(Expression<Func<T, object?>> criteria, SortingDirection direction);
    void ClearSorting();
}

public interface IHasSorting
{
    IReadOnlyList<KeyValuePair<string, SortingDirection>> Sorting { get; }
    void LoadSortingQuery(SortingQuery query);
}
