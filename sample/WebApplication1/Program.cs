using Bogus;
using EFCore.BulkExtensions;
using fbognini.Infrastructure.Multitenancy;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Repository;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Extensions;
using WebApplication1.Infrastructure.Repositorys;
using WebApplication1.SearchCriterias;
using fbognini.Infrastructure.Outbox;
using fbognini.Core.Domain;
using fbognini.Core.Domain.Query;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatROutboxMessagesPublisher(builder.Configuration);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    //options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMultiTenancy();

app.MapGet("/books", async (string? title, IWebApplication1Repository repository) =>
{
    var criteria = new BookSearchCriteria()
    {
        Title = title
    };
    criteria.Args.Includes.Add(x => x.Author);
    criteria.Args.ThrowExceptionIfNull = true;

    var books = await repository.GetFirstAsync<Book>(criteria);
    return books;
})
.WithName("GetBooks")
.WithOpenApi();

app.MapPost("/books", async (Book book, IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    await repository.CreateAsync(book, cancellationToken);
    await repository.SaveAsync(cancellationToken);
    return book;
})
.WithName("CreateBook")
.WithOpenApi();

app.MapGet("/authors", async (IWebApplication1Repository repository) =>
{
    var criteria = new AuthorSearchCriteria();

    var authors = await repository.GetAllAsync<Author>(criteria);
    return authors;
})
.WithName("GetAuthors")
.WithOpenApi();

app.MapPost("/authors", async (Author postAuthor, IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var author = Author.Create(postAuthor.FirstName, postAuthor.LastName);

    await repository.CreateAsync(author, cancellationToken);
    await repository.SaveAsync(cancellationToken);

    return author;
})
.WithName("CreateAuthor")
.WithOpenApi();

app.MapGet("/authors/{id}", async (int id, IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var args = new SelectArgs<Author>()
    {
        ThrowExceptionIfNull = true
    };
    args.Includes.Add(x => x.Books);
    var entity = await repository.GetByIdAsync<Author>(id, args, cancellationToken: cancellationToken);
    return entity;
})
.WithName("GetAuthor")
.WithOpenApi();


app.MapPut("/authors/{id}", async (int id, Author author, IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var args = new SelectArgs<Author>()
    {
        ThrowExceptionIfNull = true,
        Track = true,
    };
    args.Includes.Add(x => x.Books);
    var entity = await repository.GetByIdAsync<Author>(id, args, cancellationToken: cancellationToken);
    entity!.FirstName = author.FirstName;
    entity.LastName = author.LastName;
    entity.Books = new List<Book>();

    repository.Update(entity);
    await repository.SaveAsync(cancellationToken);
    return entity;
})
.WithName("UpdateAuthor")
.WithOpenApi();

app.MapDelete("/authors/{id}", async (int id, IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var entity = await repository.GetByIdAsync<Author>(id, cancellationToken: cancellationToken);
    if (entity != null)
    {
        repository.Delete(entity);
        await repository.SaveAsync(cancellationToken);
    }
    return entity;
})
.WithName("DeleteAuthor")
.WithOpenApi();

app.MapPost("/authors/bulk", async (IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var testUsers = new Faker<Author>()
        .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
        .RuleFor(u => u.LastName, (f, u) => f.Name.LastName());

    var authors = testUsers.Generate(10);

    await repository.MassiveInsertAsync(authors, cancellationToken: cancellationToken);
})
.WithName("BulkAuthor")
.WithOpenApi();

app.MapPost("/authors/bulk-update", async (IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var testUsers = new Faker<Author>()
        .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
        .RuleFor(u => u.LastName, (f, u) => f.Name.LastName());

    var authors = testUsers.Generate(10);

    var config = new BulkConfig()
    {
        SetOutputIdentity = true,
    };
    await repository.MassiveInsertAsync(authors, config, cancellationToken: cancellationToken);

    foreach (var author in authors)
    {
        author.LastName = "Modificato";
    }

    await repository.MassiveUpdateAsync(authors, cancellationToken: cancellationToken);
})
.WithName("BulkAuthorUpdate")
.WithOpenApi();


app.MapPost("/authors/batch-delete", async (string lastname, IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var criteria = new AuthorSearchCriteria()
    {
        LastName = lastname
    };

    await repository.BatchDeleteAsync(criteria, cancellationToken: cancellationToken);
})
.WithName("BatchAuthorDelete")
.WithOpenApi();

app.MapPost("/authors/bulk-delete", async (string lastname, IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var criteria = new AuthorSearchCriteria()
    {
        LastName = lastname
    };

    var authors = await repository.GetAllAsync<Author>(criteria);

    await repository.MassiveDeleteAsync(authors, cancellationToken: cancellationToken);
})
.WithName("BulkAuthorDelete")
.WithOpenApi();



app.MapPost("/authors/bulk-upsert", async (IWebApplication1Repository repository, CancellationToken cancellationToken) =>
{
    var testUsers = new Faker<Author>()
        .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
        .RuleFor(u => u.LastName, (f, u) => f.Name.LastName());

    var authors = testUsers.Generate(10);

    var config = new BulkConfig()
    {
        SetOutputIdentity = true,
    };
    await repository.MassiveInsertAsync(authors, config, cancellationToken: cancellationToken);

    foreach (var author in authors)
    {
        author.LastName = "Upsert";
    }

    var newAuthors = testUsers.Generate(10);

    var allAuthors = new List<Author>();
    allAuthors.AddRange(authors);
    allAuthors.AddRange(newAuthors);

    await repository.MassiveUpsertAsync(allAuthors, cancellationToken: cancellationToken);
})
.WithName("BulkAuthorUpsert")
.WithOpenApi();

app.Run();