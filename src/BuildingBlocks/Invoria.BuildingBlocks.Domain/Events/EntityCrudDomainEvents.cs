using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.BuildingBlocks.Domain.Events;

public abstract class EntityCreatedDomainEvent<TEntity, TId> : DomainEvent
    where TEntity : IEntity<TId>
{
    protected EntityCreatedDomainEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}

public abstract class EntityUpdatedDomainEvent<TEntity, TId> : DomainEvent
    where TEntity : IEntity<TId>
{
    protected EntityUpdatedDomainEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}

public abstract class EntityDeletedDomainEvent<TEntity, TId> : DomainEvent
    where TEntity : IEntity<TId>
{
    protected EntityDeletedDomainEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}

