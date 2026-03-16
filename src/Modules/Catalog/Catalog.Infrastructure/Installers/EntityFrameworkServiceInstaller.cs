using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Infrastructure.EntityFramework;
using Invoria.Catalog.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Infrastructure.Installers
{
    public class EntityFrameworkServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddInvoriaDbContext<CatalogDbContext>(cfg =>
            {
                cfg.UseSqlServer(configuration.GetConnectionString("Default"), sqlCfg =>
                    sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                );
            });

            services.AddTransient(typeof(ICatalogRepository<>), typeof(CatalogRepository<>));
        }
    }
}
