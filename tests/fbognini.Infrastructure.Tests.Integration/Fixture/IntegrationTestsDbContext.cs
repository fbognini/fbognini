using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Tests.Integration.Fixture.Entities;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Tests.Integration.Fixture;

public class IntegrationTestsDbContext : AuditableDbContext<IntegrationTestsDbContext>
{
    public IntegrationTestsDbContext(DbContextOptions<IntegrationTestsDbContext> options, ICurrentUserService currentUserService, ITenantInfo currentTenant) : base(options, currentUserService, currentTenant)
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
