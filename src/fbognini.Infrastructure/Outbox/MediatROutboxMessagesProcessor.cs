using fbognini.Core.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public class MediatROutboxMessagesProcessor : IOutboxMessageProcessor
{
    private readonly IPublisher publisher;
    private readonly ILogger<MediatROutboxMessagesProcessor> logger;

    public MediatROutboxMessagesProcessor(IPublisher mediator, ILogger<MediatROutboxMessagesProcessor> logger)
    {
        this.publisher = mediator;
        this.logger = logger;
    }

    public async Task Process(OutboxMessage outboxMessage, CancellationToken cancellation)
    {
        var notification = JsonConvert.DeserializeObject<IDomainEvent>(outboxMessage.Content, DomainEventExtensions.JsonSerializerSettings);
        if (notification is not null)
        {
            await publisher.Publish(notification, cancellation);
        }
        else
        {
            logger.LogWarning("Outbox {OutboxId} can't be deserialized in a IDomainEvent", outboxMessage.Id);
        }
    }
}