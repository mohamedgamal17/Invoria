using Hangfire;
using Hangfire.SqlServer;
using Invoria.BackgroundJob.Core;
using Microsoft.Extensions.DependencyInjection;

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

        builder.Services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        builder.Services.AddScoped<IRecurringJobScheduler, HangfireRecurringJobScheduler>();

        return builder;
    }
}
