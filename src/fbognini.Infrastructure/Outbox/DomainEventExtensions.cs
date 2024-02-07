using fbognini.Core.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace fbognini.Infrastructure.Outbox
{
    public static class DomainEventExtensions
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public static OutboxMessage ToOutboxMessage(this IDomainEvent domainEvent)
        {
            return new OutboxMessage(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    Assembly.GetEntryAssembly()!.GetName().Name!,
                    domainEvent.GetType().Name,
                    JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings));
        }
    }
}
