using Invoria.BuildingBlocks.Application.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Application.Stock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Infrastructure.Installers
{
    public class ApplicationServceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddTransient<IProductStockService, ProductStockService>()
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly))
                .RegisterFactoriesFromAssembly(Application.AssemblyReference.Assembly);
        }
    }
}

