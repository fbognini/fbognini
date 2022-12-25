using fbognini.Core.Data;
using fbognini.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OutputCaching;
using System.Text.Json.Serialization;
using WebApplication1.Application.Interfaces.Repositorys;
using WebApplication1.Application.SearchCriterias;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

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

app.MapGet("/books", async (string? title, IWebApplication1RepositoryAsync repository) =>
{
    var criteria = new BookSearchCriteria()
    {
        Title = title,
    };
    criteria.Includes.Add(x => x.Author);

    var books = await repository.GetAllAsync<Book>(criteria);
    return books;
})
.WithName("GetBooks")
.WithOpenApi();

app.MapPost("/books", async (Book book, IWebApplication1RepositoryAsync repository, CancellationToken cancellationToken) =>
{
    await repository.CreateAsync(book, cancellationToken);
    await repository.SaveAsync(cancellationToken);
    return book;
})
.WithName("CreateBook")
.WithOpenApi();

app.MapGet("/authors", async (IWebApplication1RepositoryAsync repository) =>
{
    var criteria = new AuthorSearchCriteria();

    var authors = await repository.GetAllAsync<Author>(criteria);
    return authors;
})
.WithName("GetAuthors")
.WithOpenApi();

app.MapPost("/authors", async (Author author, IWebApplication1RepositoryAsync repository, CancellationToken cancellationToken) =>
{
    await repository.CreateAsync(author, cancellationToken);
    await repository.SaveAsync(cancellationToken);
    return author;
})
.WithName("CreateAuthor")
.WithOpenApi();

app.Run();