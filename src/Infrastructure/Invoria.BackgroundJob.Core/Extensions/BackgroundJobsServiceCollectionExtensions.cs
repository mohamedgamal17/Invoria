using Invoria.BackgroundJob.Core.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJob.Core;

public static class BackgroundJobsServiceCollectionExtensions
{
    public static IBackgroundJobsBuilder AddBackgroundJobs(
        this IServiceCollection services)
    {
        services.AddSingleton<JobExecutionContextAccessor>();
        services.AddSingleton<IJobExecutionContextAccessor>(
            sp => sp.GetRequiredService<JobExecutionContextAccessor>());

        return new BackgroundJobsBuilder(services);
    }
}

