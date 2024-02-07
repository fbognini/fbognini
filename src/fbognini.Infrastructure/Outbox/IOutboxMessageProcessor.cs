using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public interface IOutboxMessageProcessor
{
    Task Process(OutboxMessage outboxMessage, CancellationToken cancellation);
}
