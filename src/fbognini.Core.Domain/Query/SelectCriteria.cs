using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
    }

    public class SearchCriteria<TEntity> : QueryableAuditableCriteria<TEntity>
        where TEntity : IHaveId<long>, IHaveLastUpdated
    {
        public SelectArgs<TEntity> Args { get; private init; } = new() { Track = false };
    }
}
