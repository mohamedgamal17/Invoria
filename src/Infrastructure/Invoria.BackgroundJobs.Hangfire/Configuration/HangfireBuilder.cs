using Invoria.BackgroundJob.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Hangfire;

public class HangfireBuilder : BackgroundJobsBuilder
{
    internal HangfireBuilder(IServiceCollection services) : base(services)
    {
    }
}
