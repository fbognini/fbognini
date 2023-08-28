using fbognini.Core.Entities;

namespace fbognini.Infrastructure.Tests.Integration.Fixture;

public class Book : AuditableEntityWithIdentity<string>
{
    public int AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;

    public Author? Author { get; set; }
}
