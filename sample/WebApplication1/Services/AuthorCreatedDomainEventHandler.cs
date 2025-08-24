using fbognini.Core.Domain;
using fbognini.Infrastructure.Outbox;
using MediatR;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApplication1.Domain.Entities;
using WebApplication1.Domain.Entities.Events;
using WebApplication1.Infrastructure.Repositorys;

namespace WebApplication1.Services;

internal sealed class AuthorCreatedDomainEventHandler : INotificationHandler<AuthorCreatedEvent>
{
    private readonly IWebApplication1Repository _repository;

    public AuthorCreatedDomainEventHandler(IWebApplication1Repository repository)
    {
        _repository = repository;
    }

    public async Task Handle(AuthorCreatedEvent notification, CancellationToken cancellationToken)
    {
        var author = await _repository.GetByIdAsync<Author>(notification.Id);

        author!.NoOfBooks += 1;

        var bookId = Guid.NewGuid().ToString()[..13];
        var book = Book.Create(bookId, notification.Id, "Creato da notification");

        _repository.Create(book);

        await _repository.SaveAsync(cancellationToken);

    }
}
