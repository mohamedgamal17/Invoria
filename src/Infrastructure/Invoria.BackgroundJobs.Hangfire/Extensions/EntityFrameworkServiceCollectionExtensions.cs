using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJobs.Hangfire;
using Invoria.BackgroundJobs.Hangfire.EntityFramework;
using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Invoria.BackgroundJobs.Hangfire.Extensions;

public static class EntityFrameworkServiceCollectionExtensions
{
    public static HangfireBuilder AddBackgroundJobDbContext<TContext>(
        this HangfireBuilder builder,
        Action<DbContextOptionsBuilder> configure)
        where TContext : InvoriaDbContext<TContext>
    {
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.AddInvoriaDbContext<TContext>(configure);
        builder.Services.TryAddScoped<IJobCheckpointStore, JobCheckpointStore<TContext>>();

        return builder;
    }
}
