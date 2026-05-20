using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Reporting.Domain.Repositories;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Invoria.Reporting.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Reporting.Infrastructure.Installers
{
    public class EntityFrameworkServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddInvoriaDbContext<ReportingDbContext>(cfg =>
            {
                cfg.UseSqlServer(configuration.GetConnectionString("Default"), sqlCfg =>
                    sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                );
            });

            services.AddTransient(typeof(IReportingRepository<>), typeof(ReportingRepository<>));
            services.AddTransient<IReportedOrderRepository, ReportedOrderRepository>();
        }
    }
}
