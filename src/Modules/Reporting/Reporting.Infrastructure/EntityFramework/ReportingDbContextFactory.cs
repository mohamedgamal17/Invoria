using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Invoria.Reporting.Infrastructure.EntityFramework;

/// <summary>
/// Design-time factory for EF Core tools (migrations) without requiring the API host project.
/// </summary>
public sealed class ReportingDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
{
    public ReportingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReportingDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=Invoria;Trusted_Connection=True;MultipleActiveResultSets=true");

        var hookEngine = new DbHookEngine(
            Array.Empty<IBeforeDbHookSave>(),
            Array.Empty<IAfterDbHookSave>());

        return new ReportingDbContext(optionsBuilder.Options, hookEngine);
    }
}
