using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.Catalog.Infrastructure.EntityFramework
{
    public class CatalogDbContext : InvoriaDbContext<CatalogDbContext>
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options, IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

    }
}
