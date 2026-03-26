using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Invoria.Inventory.Application.Tests
{
    public class InventoryTestFixture : TestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.InstallModule<InventoryTestModuleInstaller>(Configuration);
        }

        protected override async Task BeforeAllTestRunAsync()
        {
            await ServiceProvider.RunModulesBootstrapperAsync();
        }
    }
}

