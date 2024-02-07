using fbognini.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public class OutboxMessageEntry
{
    public OutboxMessageEntry(IDomainEvent? domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public OutboxMessageEntry(IDomainPreEvent? domainPreEvent)
    {
        DomainPreEvent = domainPreEvent;
    }

    public IDomainEvent? DomainEvent { get; init; }

    public IDomainPreEvent? DomainPreEvent { get; init; }
}
