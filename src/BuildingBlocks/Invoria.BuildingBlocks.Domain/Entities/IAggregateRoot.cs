using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.BuildingBlocks.Domain.Entities;


public interface IAggregateRoot<out TId> : IEntity<TId>
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}


public interface IAggregateRoot : IAggregateRoot<string> , IEntity 
{

}

