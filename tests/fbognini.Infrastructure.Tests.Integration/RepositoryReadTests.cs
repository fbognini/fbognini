using fbognini.Core.Domain;
using fbognini.Core.Domain.Query;
using fbognini.Core.Domain.Query.Pagination;
using fbognini.Infrastructure.Repository;
using fbognini.Infrastructure.Tests.Integration.Fixture;
using fbognini.Infrastructure.Tests.Integration.Fixture.Entities;
using fbognini.Infrastructure.Tests.Integration.Fixture.Entities.Seeds;
using fbognini.Infrastructure.Tests.Integration.Fixture.SearchCriterias;
using FluentAssertions;

namespace fbognini.Infrastructure.Tests.Integration;

public class RepositoryReadTests : IClassFixture<FullDatabaseFixture>
{

    private readonly IRepositoryAsync repository;

    public RepositoryReadTests(FullDatabaseFixture databaseFixture)
    {
        repository = databaseFixture.Repository;
    }


    [Fact]
    public async Task GetSearchResultsAsync_ReturnFullList_WhenFirstPage()
    {
        var pageSize = 8;

        var criteria = new AuthorSearchCriteria();
        criteria.LoadPaginationOffsetQuery(new PaginationOffsetQuery(pageSize, 1));
        var paginatedAuthors = await repository.GetSearchResultsAsync<Author>(criteria);

        paginatedAuthors.Pagination.Total.Should().Be(AuthorSeed.Total);
        paginatedAuthors.Pagination.PageSize.Should().Be(pageSize);
        paginatedAuthors.Pagination.PageNumber.Should().Be(1);

        paginatedAuthors.Items.Count.Should().Be(pageSize);
    }


    [Fact]
    public async Task GetSearchResultsAsync_ReturnNotFullList_WhenLastPage()
    {
        var pageSize = 8;
        var pageNumber = (int)Math.Ceiling((double)AuthorSeed.Total / pageSize);

        var criteria = new AuthorSearchCriteria();
        criteria.LoadPaginationOffsetQuery(new PaginationOffsetQuery(pageSize, pageNumber));
        var paginatedAuthors = await repository.GetSearchResultsAsync<Author>(criteria);

        paginatedAuthors.Pagination.Total.Should().Be(AuthorSeed.Total);
        paginatedAuthors.Pagination.PageSize.Should().Be(pageSize);
        paginatedAuthors.Pagination.PageNumber.Should().Be(pageNumber);

        paginatedAuthors.Items.Count.Should().Be(AuthorSeed.Total % pageSize);
    }


    [Fact]
    public async Task GetSearchResultsAsync_ReturnEmptyList_WhenNotExistingPage()
    {
        var pageSize = 8;
        var pageNumber = (int)Math.Ceiling((double)AuthorSeed.Total / pageSize) + 1;

        var criteria = new AuthorSearchCriteria();
        criteria.LoadPaginationOffsetQuery(new PaginationOffsetQuery(pageSize, pageNumber));
        var paginatedAuthors = await repository.GetSearchResultsAsync<Author>(criteria);

        paginatedAuthors.Pagination.Total.Should().Be(AuthorSeed.Total);
        paginatedAuthors.Pagination.PageSize.Should().Be(pageSize);
        paginatedAuthors.Pagination.PageNumber.Should().Be(pageNumber);

        paginatedAuthors.Items.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetSearchResultsAsync_ReturnFilteredList_WhenFilterByLastName()
    {
        var pageSize = 8;
        var pageNumber = 1;

        var criteria = new AuthorSearchCriteria()
        {
            LastName = AuthorSeed.FirstLastName
        };
        criteria.LoadPaginationOffsetQuery(new PaginationOffsetQuery(pageSize, pageNumber));
        var paginatedAuthors = await repository.GetSearchResultsAsync<Author>(criteria);

        paginatedAuthors.Pagination.Total.Should().Be(1);
        paginatedAuthors.Pagination.PageSize.Should().Be(pageSize);
        paginatedAuthors.Pagination.PageNumber.Should().Be(pageNumber);

        paginatedAuthors.Items.Count.Should().Be(1);
        paginatedAuthors.Items.First().LastName.Should().Be(AuthorSeed.FirstLastName);
    }

    [Fact]
    public async Task GetAllAsync_ReturnFilteredList_WhenFilterByLastName()
    {
        var criteria = new AuthorSearchCriteria()
        {
            LastName = AuthorSeed.FirstLastName
        };
        criteria.AddSorting(s => s.LastName, SortingDirection.ASCENDING);
        var authors = await repository.GetAllAsync<Author>(criteria);

        authors.Count.Should().Be(1);
        authors.First().LastName.Should().Be(AuthorSeed.FirstLastName);
    }

    [Fact]
    public async Task GetAllAsync_ReturnSortedList_WhenSortByLastName()
    {
        var criteria = new AuthorSearchCriteria();
        criteria.AddSorting(x => x.LastName, SortingDirection.ASCENDING);
        var authors = await repository.GetAllAsync<Author>(criteria);

        authors.First().LastName.Should().Be(AuthorSeed.FirstLastName);
        authors.Last().LastName.Should().Be(AuthorSeed.LastLastName);
    }

    [Fact]
    public async Task CountAsync_ReturnListCount()
    {
        var criteria = new AuthorSearchCriteria();
        var authorsCount = await repository.CountAsync<Author>(criteria);

        authorsCount.Should().Be(AuthorSeed.Total);
    }

    [Fact]
    public async Task AnyAsync_ReturnTrue_WhenNotEmptyTable()
    {
        var criteria = new AuthorSearchCriteria();
        var authorsAny = await repository.AnyAsync<Author>(criteria);

        authorsAny.Should().Be(true);
    }

    [Fact]
    public async Task AnyAsync_ReturnFalse_WhenFilterByNotExistingLastName()
    {
        var criteria = new AuthorSearchCriteria()
        {
            LastName = "1234"
        };
        var authorsAny = await repository.AnyAsync<Author>(criteria);

        authorsAny.Should().Be(false);
    }

    [Fact]
    public async Task AnyAsync_ReturnFalse_WhenEmptyTable()
    {
        var any = await repository.AnyAsync<EmptyEntity>();

        any.Should().Be(false);
    }
}
