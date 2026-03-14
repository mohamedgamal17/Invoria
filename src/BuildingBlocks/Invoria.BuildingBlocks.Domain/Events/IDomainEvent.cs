namespace Invoria.BuildingBlocks.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}

