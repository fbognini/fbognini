using fbognini.Core.Domain;
using fbognini.Infrastructure.Outbox;
using MediatR;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApplication1.Domain.Entities.Events;

namespace WebApplication1.Services;

internal sealed class BookingReservedDomainEventHandler : INotificationHandler<AuthorCreatedEvent>
{

    public BookingReservedDomainEventHandler()
    {
    }

    public async Task Handle(AuthorCreatedEvent notification, CancellationToken cancellationToken)
    {
        await Task.Delay(10000, cancellationToken);
    }
}
