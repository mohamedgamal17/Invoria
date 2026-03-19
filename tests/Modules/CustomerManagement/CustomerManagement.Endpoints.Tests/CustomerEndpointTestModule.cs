using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Endpoints.Tests;
using Invoria.Endpoints.Tests.Logger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Invoria.CustomerManagement.Endpoints.Tests
{
    public class CustomerEndpointTestModule : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallModule<CustomerEndpointTestModule>(configuration);
        }
    }

    public class CustomerModuleWebApplicationFactory : TestWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(ILogger<>));
                services.AddSingleton(typeof(ILogger<>), typeof(NUnitLogger<>));
            });
        }
    }
}

