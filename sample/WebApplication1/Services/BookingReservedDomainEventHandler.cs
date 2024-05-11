using fbognini.Core.Domain;
using fbognini.Infrastructure.Outbox;
using MediatR;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApplication1.Domain.Entities.Events;
using WebApplication1.Infrastructure.Repositorys;

namespace WebApplication1.Services;

internal sealed class BookingReservedDomainEventHandler : INotificationHandler<AuthorCreatedEvent>
{
    private readonly IWebApplication1Repository _repository;

    public BookingReservedDomainEventHandler(IWebApplication1Repository repository)
    {
        _repository = repository;
    }

    public async Task Handle(AuthorCreatedEvent notification, CancellationToken cancellationToken)
    {
        using (var transaction = _repository.CreateTransaction())
        {
            await Task.Delay(1000, cancellationToken);
        }

    }
}
