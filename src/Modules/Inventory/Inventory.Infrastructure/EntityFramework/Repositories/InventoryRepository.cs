using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.EntityFramework.Repositories;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Infrastructure.EntityFramework;

namespace Invoria.Inventory.Infrastructure.EntityFramework.Repositories;

public class InventoryRepository<TEntity> : EFCoreRepository<TEntity, InventoryDbContext>, IInventoryRepository<TEntity>
    where TEntity : class, IBaseEntity
{
    public InventoryRepository(InventoryDbContext dbContext)
        : base(dbContext)
    {
    }
}
