using fbognini.Core.Domain;
using fbognini.Infrastructure.Outbox;
using MediatR;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApplication1.Domain.Entities.Events;
using WebApplication1.Infrastructure.Repositorys;

namespace WebApplication1.Services;

internal sealed class BookCreatedDomainEventHandler : INotificationHandler<BookCreatedEvent>
{
    private readonly IWebApplication1Repository _repository;

    public BookCreatedDomainEventHandler(IWebApplication1Repository repository)
    {
        _repository = repository;
    }

    public async Task Handle(BookCreatedEvent notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }
}
