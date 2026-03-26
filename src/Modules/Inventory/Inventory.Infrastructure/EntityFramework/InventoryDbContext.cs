using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.Inventory.Infrastructure.EntityFramework
{
    public class InventoryDbContext : InvoriaDbContext<InventoryDbContext>
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options, IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}

