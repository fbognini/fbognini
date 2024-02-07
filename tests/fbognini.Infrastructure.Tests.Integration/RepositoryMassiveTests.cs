using EFCore.BulkExtensions;
using fbognini.Infrastructure.Repository;
using fbognini.Infrastructure.Tests.Integration.Fixture;
using fbognini.Infrastructure.Tests.Integration.Fixture.Entities.Seeds;
using FluentAssertions;

namespace fbognini.Infrastructure.Tests.Integration;

public class RepositoryMassiveTests : IClassFixture<FullDatabaseFixture>
{

    private readonly IRepositoryAsync repository;
    private readonly IntegrationTestsDbContext context;

    public RepositoryMassiveTests(FullDatabaseFixture databaseFixture)
    {
        repository = databaseFixture.Repository;
        context = databaseFixture.DbContext;
    }

    [Fact]
    public async Task MassiveInsertAsync_InsertFullList_WhenOk()
    {
        var authors = AuthorSeed.Faker.Generate(50);

        var cancellationToken = new CancellationToken();

        var bulkConfig = new BulkConfig()
        {
            SetOutputIdentity = true,
        };
        await repository.MassiveInsertAsync(authors, bulkConfig, cancellationToken: cancellationToken);

        var ids = authors.Select(x => x.Id).ToList();

        var savedAuthors = context.Authors.Where(x => ids.Contains(x.Id)).ToList();

        savedAuthors.Count.Should().Be(authors.Count);
    }

    [Fact]
    public async Task MassiveInsertAsync_PopulateAuditableColumns_WhenAuditableEntityInserted()
    {
        var authors = AuthorSeed.Faker.Generate(50);

        var cancellationToken = new CancellationToken();

        var bulkConfig = new BulkConfig()
        {
            SetOutputIdentity = true,
        };
        await repository.MassiveInsertAsync(authors, bulkConfig, cancellationToken: cancellationToken);

        authors.ForEach(author =>
        {
            author.Id.Should().BeGreaterThan(0);
            author.Created.Should().NotBe(DateTime.MinValue);
            author.LastUpdated.Should().NotBe(DateTime.MinValue);
        });
    }
}
