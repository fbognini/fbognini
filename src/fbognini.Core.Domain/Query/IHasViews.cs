using fbognini.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace fbognini.Core.Domain.Query;

public interface IHasViews<TEntity>
{
    List<string> AllIncludes
    {
        get
        {
            var allViews = Includes.Select(x => x.GetPropertyPath(true)).ToList();
            allViews.AddRange(IncludeStrings);

            return allViews;
        }
    }

    List<Expression<Func<TEntity, object?>>> Includes { get; }
    List<string> IncludeStrings { get; }
}
