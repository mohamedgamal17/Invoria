using Invoria.BackgroundJobs.Hangfire.Extensions;
using Invoria.BackgroundJobs.Infrastructure.EntityFramework;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Infrastructure.Installers;

public class EntityFrameworkServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddInvoriaBackgroundJobsEntityFramework<BackgroundJobsDbContext>(cfg =>
        {
            cfg.UseSqlServer(connectionString, sqlCfg =>
                sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });
    }
}
