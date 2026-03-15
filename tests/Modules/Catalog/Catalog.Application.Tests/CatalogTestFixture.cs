using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Modules.Catalog.Application.Tests
{
    public class CatalogTestFixture : TestFixture
    {

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.InstallModule<CatalogTestModuleInstaller>(Configuration);
        }

        protected override async Task BeforeAllTestRunAsync()
        {
           await  ServiceProvider.RunModulesBootstrapperAsync();
        }
    }
}
