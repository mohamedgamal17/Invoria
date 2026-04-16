using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.Procurement.Infrastructure.EntityFramework
{
    public class ProcurementDbContext : InvoriaDbContext<ProcurementDbContext>
    {
        public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options, IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
