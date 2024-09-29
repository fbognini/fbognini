using fbognini.Core.Domain;
using WebApplication1.Domain.Entities.Events;

namespace WebApplication1.Domain.Entities
{
    public class Book : AuditableEntityWithIdentity<string>
    {
        public required int AuthorId { get; set; }

        public required string Title { get; set; }


        public static Book Create(string id, int authorId, string title)
        {
            var book = new Book() { Id = id, AuthorId = authorId, Title = title };

            book.RaiseDomainEvent(new BookCreatedEvent(id));

            return book;
        }

        public Author? Author { get; set; }
    }
}
