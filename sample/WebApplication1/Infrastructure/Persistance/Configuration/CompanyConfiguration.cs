using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Infrastructure.Persistance.Configuration
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.ToTable(nameof(WebApplication1DbContext.Books), "dbo");

            builder.Property(x => x.Id)
                .HasMaxLength(13);

            builder.Property(x => x.Title)
                .HasMaxLength(200);
        }
    }
}
