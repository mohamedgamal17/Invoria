using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJob.Core;

public class BackgroundJobsBuilder : IBackgroundJobsBuilder
{
    public IServiceCollection Services { get; }

    public BackgroundJobsBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
