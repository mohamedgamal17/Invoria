using Hangfire;
using Hangfire.SqlServer;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Middlewares;
using Invoria.BackgroundJobs.Hangfire.Execution;
using Invoria.BackgroundJobs.Hangfire.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Invoria.BackgroundJobs.Hangfire;

public static class HangfireBuilderExtensions
{
    public static HangfireBuilder UseHangfire(
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

        builder.Services.TryAddSingleton<IJobExecutionContextAccessor, JobExecutionContextAccessor>();
        builder.Services.TryAddSingleton<JobExecutionContextAccessor>();
        builder.Services.AddSingleton<IJobMiddleware, JobExecutionContextMiddleware>();
        builder.Services.TryAddSingleton<IJobMiddlewarePipeline, HangfireJobMiddlewarePipeline>();
        builder.Services.TryAddTransient<HangfireJobDispatcher>();
        builder.Services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        builder.Services.AddScoped<IRecurringJobScheduler, HangfireRecurringJobScheduler>();
        builder.Services.AddHangfireServer();

        return new HangfireBuilder(builder.Services);
    }
}
