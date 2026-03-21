using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Infrastructure.Installers
{
    public class EntityFrameworkServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddInvoriaDbContext<OrderingDbContext>(cfg =>
            {
                cfg.UseSqlServer(configuration.GetConnectionString("Default"), sqlCfg =>
                    sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                );
            });
        }
    }
}
