using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.BackgroundJobs.Hangfire.EntityFramework;

public abstract class HangfireDbContext<TContext> : InvoriaDbContext<TContext>
    where TContext : DbContext
{
    protected HangfireDbContext(
        DbContextOptions<TContext> options,
        IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
