using Invoria.BackgroundJobs.Hangfire.EntityFramework;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BackgroundJobs.Infrastructure.EntityFramework;

public class BackgroundJobsDbContext : HangfireDbContext<BackgroundJobsDbContext>
{
    public BackgroundJobsDbContext(
        DbContextOptions<BackgroundJobsDbContext> options,
        IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
    {
    }
}
