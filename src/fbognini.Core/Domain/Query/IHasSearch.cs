namespace fbognini.Core.Domain.Query;

public interface IHasSearch<TEntity>
{
    Search<TEntity> Search { get; }
}
