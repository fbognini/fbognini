using fbognini.Core.Entities;
using fbognini.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace fbognini.Core.Data
{
    public interface IRepositoryArgs<TEntity>: ITrack, IHasViews<TEntity>
    {

    }

    public interface IHasViews<TEntity>
    {
        List<string> AllIncludes
        {
            get
            {
                var allViews = Includes.Select(x => PropertyExtensions.GetPropertyPath(x, true)).ToList();
                allViews.AddRange(IncludeStrings);

                return allViews;
            }
        }

        List<Expression<Func<TEntity, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
    }

    public interface ITrack
    {
        bool Track { get; set; }
    }

    public interface IHasSinceOffset : IHasOffset
    {
        long? Since { get; }
        int? AfterId { get; }
    }
}
