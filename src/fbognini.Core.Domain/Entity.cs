using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Domain;

public interface IEntity
{

}

public class Entity: IEntity
{
    public List<IDomainEvent> _domainEvents = new();
    public List<IDomainPreEvent> _domainPreEvents = new();
    public List<IDomainMemoryEvent> _domainMemoryEvents = new();

    public List<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();
    public List<IDomainPreEvent> GetDomainPreEvents() => _domainPreEvents.ToList();
    public List<IDomainMemoryEvent> GetDomainMemoryEvents() => _domainMemoryEvents.ToList();

    public void ClearDomainEvents() => _domainEvents.Clear();
    public void ClearDomainPreEvents() => _domainPreEvents.Clear();
    public void ClearDomainMemoryEvents() => _domainMemoryEvents.Clear();

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    [Obsolete("Use RaiseDomainPreEvent")]
    public void RaisDomainPreEvent(IDomainPreEvent domainPreEvent)
    {
        _domainPreEvents.Add(domainPreEvent);
    }

    public void RaiseDomainPreEvent(IDomainPreEvent domainPreEvent)
    {
        _domainPreEvents.Add(domainPreEvent);
    }

    [Obsolete("Use RaiseDomainMemoryEvent")]
    public void RaisDomainMemoryEvent(IDomainMemoryEvent domainMemoryEvent)
    {
        _domainMemoryEvents.Add(domainMemoryEvent);
    }

    public void RaiseDomainMemoryEvent(IDomainMemoryEvent domainMemoryEvent)
    {
        _domainMemoryEvents.Add(domainMemoryEvent);
    }
}
