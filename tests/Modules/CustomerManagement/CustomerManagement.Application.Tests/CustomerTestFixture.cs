using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Invoria.CustomerManagement.Application.Tests
{
    public class CustomerTestFixture : TestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.InstallModule<CustomerTestModuleInstaller>(Configuration);
        }

        protected override async Task BeforeAllTestRunAsync()
        {
            await ServiceProvider.RunModulesBootstrapperAsync();
        }
    }
}

