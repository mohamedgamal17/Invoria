using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJob.Core;

public static class BackgroundJobsServiceCollectionExtensions
{
    public static IBackgroundJobsBuilder AddBackgroundJobs(
        this IServiceCollection services)
    {
        return new BackgroundJobsBuilder(services);
    }
}
