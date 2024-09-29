using fbognini.Core.Domain;
using MediatR;

namespace WebApplication1.Domain.Entities.Events;

public record BookCreatedEvent(string Id): IDomainEvent, INotification;
