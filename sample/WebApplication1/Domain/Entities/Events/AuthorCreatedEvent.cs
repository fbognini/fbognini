using fbognini.Core.Domain;
using MediatR;

namespace WebApplication1.Domain.Entities.Events;

public record AuthorCreatedPreEvent(Author Author) : IDomainPreEvent
{
    public IDomainEvent ToDomainEvent()
    {
        return new AuthorCreatedEvent(Author.Id);
    }
}

public record AuthorCreatedEvent(int Id): IDomainEvent, INotification;
