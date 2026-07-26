using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Infrastructure.Installers;

public sealed class JobsServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = Invoria.BackgroundJobs.Application.AssemblyReference.Assembly;

        var jobTypes = applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i == typeof(IJob)))
            .ToList();

        foreach (var jobType in jobTypes)
        {
            services.AddTransient(jobType);
        }
    }
}
