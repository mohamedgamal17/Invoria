using Invoria.Catalog.Contracts.Services;
using Invoria.Endpoints.Tests;
using Invoria.Endpoints.Tests.Logger;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Invoria.Ordering.Endpoints.Tests;

public class OrderingModuleWebApplicationFactory : TestWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
    
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(ILogger<>));
            services.AddSingleton(typeof(ILogger<>), typeof(NUnitLogger<>));
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IProductService));
            services.AddSingleton<IProductService, EmptyListProductService>();
        });
    }
}
