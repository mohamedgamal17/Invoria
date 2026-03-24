using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Invoria.Catalog.Contracts.Services;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests
{
    public class OrderingTestFixture : TestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.InstallModule<OrderingTestModuleInstaller>(Configuration);
            RegisterProductService(services);
            RegisterCustomerService(services);
        }

        protected virtual void RegisterProductService(IServiceCollection services)
        {
            services.AddSingleton<IProductService, EmptyListProductService>();
        }

        protected virtual void RegisterCustomerService(IServiceCollection services)
        {
            services.AddSingleton<ICustomerService, EmptyListCustomerService>();
        }

        protected override async Task BeforeAllTestRunAsync()
        {
            await ServiceProvider.RunModulesBootstrapperAsync();
        }
    }
}
