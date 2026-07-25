using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJobs.Hangfire.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Hangfire.Extensions;

public static class EntityFrameworkServiceCollectionExtensions
{
    public static IServiceCollection AddInvoriaBackgroundJobsEntityFramework<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddDbContext<TContext>(configure);
        services.AddScoped<IJobCheckpointStore, JobCheckpointStore<TContext>>();

        return services;
    }
}
