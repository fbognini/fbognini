using System;
using System.Linq.Expressions;

namespace fbognini.Core.Domain.Query;

public interface IHasFilter<TEntity>
{
    Expression<Func<TEntity, bool>> ResolveFilter();
}
