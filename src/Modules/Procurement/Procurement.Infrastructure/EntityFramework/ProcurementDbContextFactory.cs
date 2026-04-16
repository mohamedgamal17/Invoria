using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Invoria.Procurement.Infrastructure.EntityFramework;

public sealed class ProcurementDbContextFactory : IDesignTimeDbContextFactory<ProcurementDbContext>
{
    public ProcurementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProcurementDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=InvoriaProcurement_Dev;Trusted_Connection=True;MultipleActiveResultSets=true");

        var hookEngine = new DbHookEngine(
            Array.Empty<IBeforeDbHookSave>(),
            Array.Empty<IAfterDbHookSave>());

        return new ProcurementDbContext(optionsBuilder.Options, hookEngine);
    }
}
