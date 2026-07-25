using Hangfire;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJobs.Hangfire;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Infrastructure.Installers;

public class BackgroundJobCoreServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddBackgroundJobs()
            .UseLoggingJobMiddleware()
            .UseHangfire(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("Default");
            });

        services.AddHangfireServer();
    }
}
