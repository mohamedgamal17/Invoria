using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Infrastructure.EntityFramework.Repositories;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Infrastructure.Installers
{
    public class EntityFrameworkServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddInvoriaDbContext<InventoryDbContext>(cfg =>
            {
                cfg.UseSqlServer(configuration.GetConnectionString("Default"), sqlCfg =>
                    sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });

            services.AddTransient(typeof(IInventoryRepository<>), typeof(InventoryRepository<>));
        }
    }
}

