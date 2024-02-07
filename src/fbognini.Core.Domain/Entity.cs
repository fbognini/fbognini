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

    public List<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();
    public List<IDomainPreEvent> GetDomainPreEvents() => _domainPreEvents.ToList();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    public void ClearDomainPreEvents()
    {
        _domainPreEvents.Clear();
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RaisDomainPreEvent(IDomainPreEvent domainPreEvent)
    {
        _domainPreEvents.Add(domainPreEvent);
    }
}
