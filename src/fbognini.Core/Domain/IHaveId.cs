namespace fbognini.Core.Domain;

public interface IHaveId<TKey>
    where TKey : notnull
{
    public TKey Id { get; set; }
}
