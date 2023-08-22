using fbognini.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebApplication1.Domain.Entities;
using Finbuckle.MultiTenant;
using fbognini.Infrastructure.Persistence;

namespace WebApplication1.Infrastructure.Persistance
{
    public class WebApplication1DbContext : AuditableDbContext<WebApplication1DbContext>
    {
        public WebApplication1DbContext(
            DbContextOptions<WebApplication1DbContext> options,
            ICurrentUserService currentUserService,
            ITenantInfo currentTenant)
            : base(options, currentUserService, currentTenant)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }
    
        public DbSet<Book> Books { get; set; }
    }
}
