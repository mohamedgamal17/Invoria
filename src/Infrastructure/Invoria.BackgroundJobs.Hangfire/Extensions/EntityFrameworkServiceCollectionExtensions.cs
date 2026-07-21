using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJobs.Hangfire.EntityFramework;
using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Hangfire.Extensions;

public static class EntityFrameworkServiceCollectionExtensions
{
    public static IServiceCollection AddInvoriaBackgroundJobsEntityFramework<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
        where TContext : InvoriaDbContext<TContext>
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddInvoriaDbContext<TContext>(configure);
        services.AddScoped<IJobCheckpointStore, JobCheckpointStore<TContext>>();

        return services;
    }
}
