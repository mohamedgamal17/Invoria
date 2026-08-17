using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Invoria.Ordering.Infrastructure.EntityFramework;

public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderingDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=InvoriaOrdering_Dev;Trusted_Connection=True;MultipleActiveResultSets=true");

        var hookEngine = new DbHookEngine(
            Array.Empty<IBeforeDbHookSave>(),
            Array.Empty<IAfterDbHookSave>());

        return new OrderingDbContext(optionsBuilder.Options, hookEngine);
    }
}