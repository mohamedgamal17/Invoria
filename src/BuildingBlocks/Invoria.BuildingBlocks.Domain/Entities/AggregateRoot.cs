using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.BuildingBlocks.Domain.Entities;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

