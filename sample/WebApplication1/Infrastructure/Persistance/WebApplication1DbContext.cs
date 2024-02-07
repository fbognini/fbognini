using fbognini.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebApplication1.Domain.Entities;
using Finbuckle.MultiTenant;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Outbox;

namespace WebApplication1.Infrastructure.Persistance
{
    public class WebApplication1DbContext : AuditableDbContext<WebApplication1DbContext>
    {
        public WebApplication1DbContext(
            DbContextOptions<WebApplication1DbContext> options,
            ICurrentUserService currentUserService,
            IOutboxMessagesListener outboxMessagesListener,
            ITenantInfo? currentTenant = null)
            : base(options, currentUserService, outboxMessagesListener, currentTenant)
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
