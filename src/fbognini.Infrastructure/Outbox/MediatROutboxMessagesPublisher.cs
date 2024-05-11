using fbognini.Core.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public class MediatROutboxMessagesPublisher : IOutboxMessagePublisher
{
    private readonly IPublisher publisher;
    private readonly ILogger<MediatROutboxMessagesPublisher> logger;

    public MediatROutboxMessagesPublisher(IPublisher mediator, ILogger<MediatROutboxMessagesPublisher> logger)
    {
        this.publisher = mediator;
        this.logger = logger;
    }

    public async Task Publish(OutboxMessage outboxMessage, CancellationToken cancellation)
    {
        var notification = JsonConvert.DeserializeObject<IDomainEvent>(outboxMessage.Content, DomainEventExtensions.JsonSerializerSettings);
        if (notification is not null)
        {
            await ProcessNotification(notification, cancellation);
            return;
        }

        logger.LogWarning("Outbox message {OutboxMessageId} can't be deserialized in a IDomainEvent", outboxMessage.Id);
    }

    public Task Publish(IDomainMemoryEvent domainMemoryEvent, CancellationToken cancellation) => ProcessNotification(domainMemoryEvent, cancellation);

    private Task ProcessNotification(INotification notification, CancellationToken cancellation) => publisher.Publish(notification, cancellation);
}