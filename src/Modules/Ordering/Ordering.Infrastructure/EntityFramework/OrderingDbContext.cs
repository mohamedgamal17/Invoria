using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.Ordering.Infrastructure.EntityFramework
{
    public class OrderingDbContext : InvoriaDbContext<OrderingDbContext>
    {
        public OrderingDbContext(
            DbContextOptions<OrderingDbContext> options,
            IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
