using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Middlewares;
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

        // TODO: Register IJobMiddlewarePipeline once the pipeline implementation is created.

        return new BackgroundJobsBuilder(services);
    }

    public static IBackgroundJobsBuilder UseLoggingJobMiddleware(
        this IBackgroundJobsBuilder builder)
    {
        builder.Services.AddSingleton<IJobMiddleware, LoggingJobMiddleware>();

        return builder;
    }
}

