using System.Reflection;
using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BackgroundJobs.Hangfire.EntityFramework;

public class BackgroundJobsDbContext : InvoriaDbContext<BackgroundJobsDbContext>
{
    public BackgroundJobsDbContext(
        DbContextOptions<BackgroundJobsDbContext> options,
        IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
