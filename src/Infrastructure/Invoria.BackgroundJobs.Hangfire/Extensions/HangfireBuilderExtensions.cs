using Hangfire;
using Hangfire.SqlServer;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Context;
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
        });

        builder.Services.TryAddSingleton<IJobExecutionContextAccessor, JobExecutionContextAccessor>();
        builder.Services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        builder.Services.AddScoped<IRecurringJobScheduler, HangfireRecurringJobScheduler>();

        return builder;
    }
}
