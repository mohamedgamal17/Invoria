namespace Invoria.BuildingBlocks.Domain.Events;

public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        OccurredOn = DateTimeOffset.UtcNow;
    }

    public DateTimeOffset OccurredOn { get; }
}

