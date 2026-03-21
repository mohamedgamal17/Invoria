using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Invoria.Ordering.Infrastructure
{
    public class OrderingModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallServiceFromAssembly(configuration, Assembly.GetExecutingAssembly());
            services.AddTransient<IModuleBootstrapper, OrderingModuleBootStrapper>();
        }
    }
}
