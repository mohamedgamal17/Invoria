using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Financial.Domain.Repositories;
using Invoria.Financial.Infrastructure.EntityFramework;
using Invoria.Financial.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Infrastructure.Installers
{
    public class EntityFrameworkServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddInvoriaDbContext<FinancialDbContext>(cfg =>
            {
                cfg.UseSqlServer(configuration.GetConnectionString("Default"), sqlCfg =>
                    sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                );
            });

            services.AddTransient(typeof(IFinancialRepository<>), typeof(FinancialRepository<>));
        }
    }
}
