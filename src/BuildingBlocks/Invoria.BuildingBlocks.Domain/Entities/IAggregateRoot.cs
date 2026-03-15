namespace Invoria.BuildingBlocks.Domain.Entities;


public interface IAggregateRoot<out TId> : IEntity<TId>
{

}


public interface IAggregateRoot : IAggregateRoot<string> , IEntity { }

