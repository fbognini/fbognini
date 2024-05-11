using fbognini.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public interface IOutboxMessagePublisher
{
    Task Publish(OutboxMessage outboxMessage, CancellationToken cancellation);
    Task Publish(IDomainMemoryEvent domainMemoryEvent, CancellationToken cancellation);
}
