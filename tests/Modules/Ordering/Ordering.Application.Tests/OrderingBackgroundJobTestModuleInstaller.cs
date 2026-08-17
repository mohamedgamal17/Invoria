using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests
{
    public class OrderingBackgroundJobTestModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallModule<OrderingTestModuleInstaller>(configuration);
        }
    }
}