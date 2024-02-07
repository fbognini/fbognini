using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace fbognini.Core.Domain.Query
{

    public abstract class BaseSelectArgs
    {
        public bool ThrowExceptionIfNull { get; set; } = false;

    }

    public class SelectArgs<TEntity> : BaseSelectArgs, IHasViews<TEntity>, IArgs
    {
        public bool Track { get; set; } = true;
        public bool IgnoreQueryFilters { get; set; } = false;
        public bool IgnoreAutoIncludes { get; set; } = false;

        public List<Expression<Func<TEntity, object?>>> Includes { get; internal init; } = new List<Expression<Func<TEntity, object?>>>();
        public List<string> IncludeStrings { get; internal init; } = new List<string>();
    }

    public class SelectArgs<TEntity, TKey> : SelectArgs<TEntity>
        where TEntity : IHaveId<TKey>
        where TKey : notnull
    {
        public SelectArgs()
        {

        }

        public SelectArgs(SelectArgs<TEntity>? args)
        {
            if (args == null)
            {
                return;
            }

            Track = args.Track;
            ThrowExceptionIfNull = args.ThrowExceptionIfNull;
            IgnoreQueryFilters = args.IgnoreQueryFilters;
            IgnoreAutoIncludes = args.IgnoreAutoIncludes;

            Includes = args.Includes;
            IncludeStrings = args.IncludeStrings;
        }

        public TKey? Id { get; set; }
    }
}
