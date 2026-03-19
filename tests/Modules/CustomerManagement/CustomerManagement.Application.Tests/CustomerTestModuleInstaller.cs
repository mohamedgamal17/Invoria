using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.CustomerManagement.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Application.Tests
{
    public class CustomerTestModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallModule<CustomerManagementModuleInstaller>(configuration);
        }
    }
}

