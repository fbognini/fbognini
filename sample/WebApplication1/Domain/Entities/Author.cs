using fbognini.Core.Domain;
using WebApplication1.Domain.Entities.Events;

namespace WebApplication1.Domain.Entities
{
    public class Author : AuditableEntityWithIdentity<int>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        public int NoOfBooks { get; set; }

        public ICollection<Book> Books { get; set; } = [];

        public static Author Create(string firstName, string lastName)
        {
            var author = new Author() { FirstName = firstName, LastName = lastName };

            author.RaiseDomainPreEvent(new AuthorCreatedPreEvent(author));

            return author;
        }
    }
}
