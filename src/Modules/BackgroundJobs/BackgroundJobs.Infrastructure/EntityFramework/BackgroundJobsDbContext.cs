using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BackgroundJobs.Infrastructure.EntityFramework;

public class BackgroundJobsDbContext : InvoriaDbContext<BackgroundJobsDbContext>
{
    public BackgroundJobsDbContext(
        DbContextOptions<BackgroundJobsDbContext> options,
        IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(Invoria.BackgroundJobs.Hangfire.EntityFramework.BackgroundJobsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
