using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Endpoints.Tests;
using Invoria.Endpoints.Tests.Utilites;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Invoria.Catalog.Endpoint.Tests
{
    public class CatalogEndpointTestModule : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallModule<CatalogEndpointTestModule>(configuration);
        }
    }

    public class CatalogModuleWebApplicationFactory : TestWebApplicationFactory
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
