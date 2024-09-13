using fbognini.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebApplication1.Domain.Entities;
using Finbuckle.MultiTenant;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Outbox;
using Microsoft.Extensions.Options;

namespace WebApplication1.Infrastructure.Persistance
{
    public class WebApplication1DbContext : AuditableDbContext<WebApplication1DbContext>
    {
        public WebApplication1DbContext(
            DbContextOptions<WebApplication1DbContext> options,
            IOptions<DatabaseSettings> databaseOptions,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IOutboxMessagesListener outboxMessagesListener,
            ITenantInfo? currentTenant = null)
            : base(options, databaseOptions, currentUserService, dateTimeProvider, outboxMessagesListener, currentTenant)
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
