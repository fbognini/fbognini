using fbognini.Infrastructure.Repository;
using fbognini.Infrastructure.Tests.Integration.Fixture;
using fbognini.Infrastructure.Tests.Integration.Fixture.Entities.Seeds;
using FluentAssertions;

namespace fbognini.Infrastructure.Tests.Integration;

public class RepositoryTransactionTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{

    private readonly IRepositoryAsync repository;
    private readonly IntegrationTestsDbContext context;

    public RepositoryTransactionTests(DatabaseFixture databaseFixture)
    {
        context = databaseFixture.DbContext;
        repository = databaseFixture.Repository;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateTransactionAsync_EntitySaved_WhenCommitAsync()
    {
        var author = AuthorSeed.Faker.Generate();

        var cancellationToken = new CancellationToken();
        using (var transaction = await repository.CreateTransactionAsync(cancellationToken))
        {
            await repository.CreateAsync(author, cancellationToken);
            await repository.SaveAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        var dbAuthor = context.Authors.FirstOrDefault(x => x.Id == author.Id);

        dbAuthor.Should().NotBeNull();
        dbAuthor.Id.Should().Be(author.Id);
    }

    [Fact]
    public async Task CreateTransactionAsync_EntityNotSaved_WhenRollbackAsync()
    {
        var author = AuthorSeed.Faker.Generate();

        var cancellationToken = new CancellationToken();
        using (var transaction = await repository.CreateTransactionAsync(cancellationToken))
        {
            await repository.CreateAsync(author, cancellationToken);
            await repository.SaveAsync(cancellationToken);

            await transaction.RollbackAsync(cancellationToken);
        }

        var dbAuthor = context.Authors.FirstOrDefault(x => x.Id == author.Id);

        dbAuthor.Should().BeNull();
    }

    [Fact]
    public async Task CreateTransactionAsync_EntityNotSaved_WhenTransactionDispose()
    {
        var author = AuthorSeed.Faker.Generate();

        var cancellationToken = new CancellationToken();
        using (var transaction = await repository.CreateTransactionAsync(cancellationToken))
        {
            await repository.CreateAsync(author, cancellationToken);
            await repository.SaveAsync(cancellationToken);
        }

        var dbAuthor = context.Authors.FirstOrDefault(x => x.Id == author.Id);

        dbAuthor.Should().BeNull();
    }
}
