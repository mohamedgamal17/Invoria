using Invoria.Endpoints.Tests;
using Invoria.Endpoints.Tests.Logger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Invoria.Ordering.Endpoints.Tests;

public class OrderingModuleWebApplicationFactory : TestWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dbName = $"Invoria_OrderingEndpointsTests_{Guid.NewGuid():N}";
        var connectionString =
            $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Trust Server Certificate=True;MultipleActiveResultSets=true";

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:Default"] = connectionString });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(ILogger<>));
            services.AddSingleton(typeof(ILogger<>), typeof(NUnitLogger<>));
        });
    }
}
