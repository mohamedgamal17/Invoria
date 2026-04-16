using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Invoria.Procurement.Application.Tests
{
    public class ProcurementTestFixture : TestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.InstallModule<ProcurementTestModuleInstaller>(Configuration);
        }

        protected override async Task BeforeAllTestRunAsync()
        {
            await ServiceProvider.RunModulesBootstrapperAsync();
        }
    }
}
