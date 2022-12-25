using fbognini.Core.Entities;

namespace WebApplication1.Domain.Entities
{
    public class Author : AuditableEntityWithIdentity<int>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
