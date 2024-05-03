using fbognini.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace fbognini.Core.Domain.Query
{
    public class SelectCriteria<TEntity> : QueryableCriteria<TEntity>
    {
        public SelectArgs<TEntity> Args { get; private init; } = new() { Track = false };


        [Obsolete("Please use Args property")]
        public List<Expression<Func<TEntity, object?>>> Includes
        {
            get => Args.Includes;
        }

        [Obsolete("Please use Args property")]
        public bool Track
        {
            get => Args.Track;
            set => Args.Track = value;
        }

        public new string GetArgsKey()
        {
            var builder = GetQueryableArgsBuilder();
            builder.Append((Args as IHasViews<TEntity>).GetArgsKey());

            return builder.ToString();
        }

        public new Dictionary<string, object?> GetArgsKeyAsDictionary()
        {
            var list = GetQueryableArgsAsDisctionaryBuilder();
            list.AddRange((Args as IHasViews<TEntity>).GetArgsKeyAsDictionary());

            return list.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public class SearchCriteria<TEntity> : QueryableAuditableCriteria<TEntity>
        where TEntity : IHaveId<long>, IHaveLastUpdated
    {
        public SelectArgs<TEntity> Args { get; private init; } = new() { Track = false };
    }
}
