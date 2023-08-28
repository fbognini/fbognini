using Castle.Core.Logging;
using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Repositorys;
using fbognini.Infrastructure.Tests.Integration.Fixture.Entities.Seeds;
using Finbuckle.MultiTenant;
using MartinCostello.SqlLocalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace fbognini.Infrastructure.Tests.Integration.Fixture;

public class DatabaseFixture : IDisposable
{
    public DbContextOptions<IntegrationTestsDbContext> DbContextOptions { get; }
    public string ConnectionString { get; }

    public IntegrationTestsDbContext DbContext => new (DbContextOptions, new IntegrationTestsCurrentUserService(), new TenantInfo());

    public RepositoryAsync<IntegrationTestsDbContext> Repository => new(DbContext, Substitute.For<ILogger<RepositoryAsync<IntegrationTestsDbContext>>>());

    public DatabaseFixture()
    {
        var databaseName = $"fbognini.Infrastructure.Tests.Integration_{Guid.NewGuid().ToString().Replace('-', '_')}";

        using (var localDB = new SqlLocalDbApi())
        {
            ConnectionString = localDB.IsLocalDBInstalled()
                ? $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={databaseName};Integrated Security=SSPI;MultipleActiveResultSets=True;Connection Timeout=300;Encrypt=False;TrustServerCertificate=True;"
                : throw new InvalidOperationException("LocalDB must be installed");
        }

        DbContextOptions = new DbContextOptionsBuilder<IntegrationTestsDbContext>()
            .UseSqlServer(ConnectionString)
            .EnableSensitiveDataLogging()
            .Options;

        using var dbContext = DbContext;
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        using var dbContext = DbContext;
        dbContext.Database.EnsureDeleted();
        GC.SuppressFinalize(this);
    }
}

public class FullDatabaseFixture : DatabaseFixture
{
    public FullDatabaseFixture()
        : base()
    {
        using var dbContext = DbContext;

        dbContext.Authors.AddRange(AuthorSeed.GetAuthors());
        dbContext.SaveChanges();
    }
}

public class IntegrationTestsCurrentUserService : ICurrentUserService
{
    public string UserId => "IntegrationTests";

    public string UserName => "IntegrationTests";

    public List<string> GetRoles()
    {
        throw new NotImplementedException();
    }

    public bool HasClaim(string type, string value)
    {
        throw new NotImplementedException();
    }
}