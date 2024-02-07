using fbognini.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public class OutboxMessageEntry
{
    internal static OutboxMessageEntry FromDomainEvent(IDomainEvent domainEvent)
    {
        var entry = new OutboxMessageEntry()
        {
            DomainEvent = domainEvent
        };

        return entry;
    }

    internal static OutboxMessageEntry FromDomainPreEvent(IDomainPreEvent domainPreEvent)
    {
        var entry = new OutboxMessageEntry()
        {
            DomainPreEvent = domainPreEvent
        };

        return entry;
    }

    internal static OutboxMessageEntry FromDomainMemoryEvent(IDomainMemoryEvent domainMemoryEvent)
    {
        var entry = new OutboxMessageEntry()
        {
            DomainMemoryEvent = domainMemoryEvent
        };

        return entry;
    }

    public IDomainEvent? DomainEvent { get; init; }
    public IDomainPreEvent? DomainPreEvent { get; init; }
    public IDomainMemoryEvent? DomainMemoryEvent { get; init; }
}
