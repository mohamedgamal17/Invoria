using Hangfire;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJobs.Hangfire;
using Invoria.BackgroundJobs.Hangfire.Extensions;
using Invoria.BackgroundJobs.Infrastructure.EntityFramework;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Infrastructure.Installers;

public class BackgroundJobCoreServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Default is required to configure the background job storage. " +
                "Set the ConnectionStrings__Default environment variable or add it to appsettings.json.");
        }

        services.AddBackgroundJobs()
            .UseLoggingJobMiddleware()
            .UseHangfire(options =>
            {
                options.ConnectionString = connectionString;
            })
            .AddBackgroundJobDbContext<BackgroundJobsDbContext>(cfg =>
            {
                cfg.UseSqlServer(connectionString, sqlCfg =>
                    sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });

    }
}
