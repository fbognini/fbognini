﻿using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Outbox;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Tests.Integration.Fixture.Entities;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace fbognini.Infrastructure.Tests.Integration.Fixture;

public class IntegrationTestsDbContext : AuditableDbContext<IntegrationTestsDbContext>
{
    public IntegrationTestsDbContext(
        DbContextOptions<IntegrationTestsDbContext> options,
        IOptions<DatabaseSettings> databaseOptions,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IOutboxMessagesListener outboxMessagesListener,
        ITenantInfo currentTenant) : base(options, databaseOptions, currentUserService, dateTimeProvider, outboxMessagesListener, currentTenant)
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
