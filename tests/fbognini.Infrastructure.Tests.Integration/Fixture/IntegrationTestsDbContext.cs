using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Outbox;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Tests.Integration.Fixture.Entities;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace fbognini.Infrastructure.Tests.Integration.Fixture;

public class IntegrationTestsDbContext : AuditableDbContext<IntegrationTestsDbContext>
{
    public IntegrationTestsDbContext(DbContextOptions<IntegrationTestsDbContext> options, ICurrentUserService currentUserService, IOutboxMessagesListener outboxMessagesListener, ITenantInfo currentTenant) : base(options, currentUserService, outboxMessagesListener, currentTenant)
    {
    }

    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<EmptyEntity> EmptyEntitys { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
