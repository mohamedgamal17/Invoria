using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Repositories;

namespace Invoria.Inventory.Domain;

public interface IInventoryRepository<T> : IRepository<T>
    where T : IBaseEntity
{
}
