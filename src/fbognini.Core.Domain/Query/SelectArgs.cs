using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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

        public virtual string GetArgsKey()
        {
            var builder = new StringBuilder();
            builder.Append(typeof(TEntity).Name);

            builder.Append((this as IHasViews<TEntity>).GetArgsKey());

            return builder.ToString();
        }

        public virtual Dictionary<string, object?> GetArgsKeyAsDictionary()
        {
            var dictionary = new List<KeyValuePair<string, object?>>
            {
                new("_Entity", typeof(TEntity).Name)
            };

            dictionary.AddRange((this as IHasViews<TEntity>).GetArgsKeyAsDictionary());

            return dictionary.ToDictionary(x => x.Key, x => x.Value);
        }
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
