using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Invoria.Inventory.Infrastructure
{
    public class InventoryModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallServiceFromAssembly(configuration, Assembly.GetExecutingAssembly());
            services.AddTransient<IModuleBootstrapper, InventoryModuleBootStrapper>();
        }
    }
}

