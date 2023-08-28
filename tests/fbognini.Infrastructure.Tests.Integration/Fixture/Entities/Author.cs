using fbognini.Core.Entities;

namespace fbognini.Infrastructure.Tests.Integration.Fixture;

public class Author : AuditableEntityWithIdentity<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public ICollection<Book>? Books { get; set; }
}
