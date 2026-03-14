namespace Invoria.BuildingBlocks.Domain.Entities;

public interface IAggregateRoot
{
}

public interface IAggregateRoot<out TId> : IEntity<TId>, IAggregateRoot
{
}

