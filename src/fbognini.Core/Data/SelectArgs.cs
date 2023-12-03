using fbognini.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace fbognini.Core.Data
{
    public interface IArgs
    {
    }

    public abstract class BaseSelectArgs
    {
        public abstract bool Track { get; set; }
        public bool ThrowExceptionIfNull { get; set; } = false;

    }

    public class SelectArgs<TEntity> : BaseSelectArgs, IHasViews<TEntity>, IArgs
    {
        public override bool Track { get; set; } = true;
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
            Includes = args.Includes;
            IncludeStrings = args.IncludeStrings;
            ThrowExceptionIfNull = args.ThrowExceptionIfNull;
        }

        public TKey? Id { get; set; }
    }
}
