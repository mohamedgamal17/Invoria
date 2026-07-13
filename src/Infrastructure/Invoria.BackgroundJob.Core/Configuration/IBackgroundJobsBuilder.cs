using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJob.Core;

public interface IBackgroundJobsBuilder
{
    IServiceCollection Services { get; }
}
