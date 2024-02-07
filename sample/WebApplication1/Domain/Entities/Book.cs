using fbognini.Core.Domain;

namespace WebApplication1.Domain.Entities
{
    public class Book : AuditableEntityWithIdentity<string>
    {
        public required int AuthorId { get; set; }
        public required string Title { get; set; }

        public Author? Author { get; set; }
    }
}
