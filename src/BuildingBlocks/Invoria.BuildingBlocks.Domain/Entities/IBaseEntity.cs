namespace Invoria.BuildingBlocks.Domain.Entities;

public interface IBaseEntity
{

}

public interface IEntity<out TId> : IBaseEntity
{
    TId Id { get; }
}


public interface IEntity : IEntity<string> { }

