using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJob.Core;

internal sealed class BackgroundJobsBuilder : IBackgroundJobsBuilder
{
    public IServiceCollection Services { get; }

    internal BackgroundJobsBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
