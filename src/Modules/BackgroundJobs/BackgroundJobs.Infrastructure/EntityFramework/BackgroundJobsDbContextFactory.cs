using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Invoria.BackgroundJobs.Infrastructure.EntityFramework;

public sealed class BackgroundJobsDbContextFactory : IDesignTimeDbContextFactory<BackgroundJobsDbContext>
{
    public BackgroundJobsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BackgroundJobsDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=Invoria_Jobs;Trusted_Connection=True;MultipleActiveResultSets=true");

        var hookEngine = new DbHookEngine(
            Array.Empty<IBeforeDbHookSave>(),
            Array.Empty<IAfterDbHookSave>());

        return new BackgroundJobsDbContext(optionsBuilder.Options, hookEngine);
    }
}
