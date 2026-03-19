using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.CustomerManagement.Infrastructure.EntityFramework
{
    public class CustomerManagementDbContext : InvoriaDbContext<CustomerManagementDbContext>
    {
        public CustomerManagementDbContext(DbContextOptions<CustomerManagementDbContext> options, IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}

