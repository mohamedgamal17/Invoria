using MediatR;

namespace Invoria.BuildingBlocks.Domain.Events;

public interface IDomainEvent : INotification
{ 
    DateTimeOffset OccurredOn { get; }
}

