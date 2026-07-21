using Hangfire;
using Hangfire.SqlServer;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Middlewares;
using Invoria.BackgroundJobs.Hangfire.EntityFramework;
using Invoria.BackgroundJobs.Hangfire.Execution;
using Invoria.BackgroundJobs.Hangfire.Extensions;
using Invoria.BackgroundJobs.Hangfire.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Invoria.BackgroundJobs.Hangfire;

public static class HangfireBuilderExtensions
{
    public static IBackgroundJobsBuilder UseHangfire(
        this IBackgroundJobsBuilder builder,
        Action<HangfireOptions>? configure = null)
    {
        var options = new HangfireOptions();
        configure?.Invoke(options);

        builder.Services.AddHangfire(config =>
        {
            if (options.ConnectionString is not null)
            {
                config.UseSqlServerStorage(options.ConnectionString, new SqlServerStorageOptions
                {
                    SchemaName = options.SchemaName ?? "Hangfire"
                });
            }

            config.UseFilter(new HangfireJobIdCaptureFilter());
        });

        RegisterCheckpointStore(builder.Services, options);

        builder.Services.TryAddSingleton<IJobExecutionContextAccessor, JobExecutionContextAccessor>();
        builder.Services.TryAddSingleton<JobExecutionContextAccessor>();
        builder.Services.AddSingleton<IJobMiddleware, JobExecutionContextMiddleware>();
        builder.Services.TryAddSingleton<IJobMiddlewarePipeline, HangfireJobMiddlewarePipeline>();
        builder.Services.TryAddTransient<HangfireJobDispatcher>();
        builder.Services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        builder.Services.AddScoped<IRecurringJobScheduler, HangfireRecurringJobScheduler>();

        return builder;
    }

    private static void RegisterCheckpointStore(IServiceCollection services, HangfireOptions options)
    {
        string connectionString = options.CheckpointsConnectionString ?? options.ConnectionString
            ?? throw new InvalidOperationException(
                "A connection string is required for the checkpoint store. " +
                "Set either CheckpointsConnectionString or ConnectionString on HangfireOptions.");

        services.AddInvoriaBackgroundJobsEntityFramework<BackgroundJobsDbContext>(cfg =>
        {
            cfg.UseSqlServer(connectionString, sqlCfg =>
                sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });
    }
}
