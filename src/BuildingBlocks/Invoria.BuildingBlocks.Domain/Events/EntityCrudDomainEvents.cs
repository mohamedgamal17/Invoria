using Invoria.BuildingBlocks.Domain.Entities;
using MediatR;

namespace Invoria.BuildingBlocks.Domain.Events;

public sealed class EntityCreatedDomainEvent<TEntity, TId> : DomainEvent, INotification
    where TEntity : class, IEntity<TId>
{
    public EntityCreatedDomainEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}

public sealed class EntityUpdatedDomainEvent<TEntity, TId> : DomainEvent, INotification
    where TEntity : class, IEntity<TId>
{
    public EntityUpdatedDomainEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}

public sealed class EntityDeletedDomainEvent<TEntity, TId> : DomainEvent, INotification
    where TEntity : class, IEntity<TId>
{
    public EntityDeletedDomainEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}
