using Invoria.BuildingBlocks.EntityFramework.Persistence;
using Invoria.Inventory.Domain;

namespace Invoria.Inventory.Infrastructure.EntityFramework;

public sealed class InventoryUnitOfWork : EfUnitOfWork<InventoryDbContext>, IInventoryUnitOfWork
{
    public InventoryUnitOfWork(InventoryDbContext context)
        : base(context)
    {
    }
}
