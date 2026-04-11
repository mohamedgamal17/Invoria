using FastEndpoints;
using FastEndpoints.Swagger;
using Invoria.Api.Infrastructure;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Invoria.Catalog.Infrastructure;
using Invoria.CustomerManagement.Infrastructure;
using Invoria.Inventory.Infrastructure;
using Invoria.Ordering.Infrastructure;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Microsoft.Extensions.Logging;
using Rebus.Routing.TypeBased;
using Rebus.Serialization.Json;
using Rebus.ServiceProvider;

namespace Invoria.Api
{
    public class ApiModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            AddInvoriaRebus(services, configuration);

            services.InstallModule<CatalogModuleInstaller>(configuration)
                .AddApplicationInfrastructure();

            services.InstallModule<CustomerManagementModuleInstaller>(configuration);
            services.InstallModule<InventoryModuleInstaller>(configuration);
            services.InstallModule<OrderingModuleInstaller>(configuration);

            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddProblemDetails();

            ConfigureSwagger(services);

            ConfigureFastEndpoint(services);

            ConfigureControllers(services);

        }

        /// <summary>
        /// One-time Rebus configuration: transport, subscriptions storage, routing, logging. Per-module handlers are registered from each module Infrastructure.
        /// </summary>
        private static void AddInvoriaRebus(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Rebus")
                ?? configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("A SQL connection string is required (ConnectionStrings:Rebus or ConnectionStrings:Default).");

            var inputQueueName = configuration["Rebus:InputQueue"]
                ?? throw new InvalidOperationException("Rebus:InputQueue must be configured.");

            var transportOptions = new SqlServerTransportOptions(connectionString, false)
                .SetEnsureTablesAreCreated(true);

            services.AddRebus(
                (configure, provider) => configure
                    .Logging(l => l.MicrosoftExtensionsLogging(provider.GetRequiredService<ILoggerFactory>()))
                    .Serialization(s => s.UseSystemTextJson())
                    .Subscriptions(s => s.StoreInSqlServer(connectionString, "RebusSubscriptions", true, true, false))
                    .Transport(t => t.UseSqlServer(transportOptions, inputQueueName))
                    .Routing(r => r.TypeBased().MapAssemblyOf<Ordering.Contracts.AssemblyReference>(inputQueueName)));
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.SwaggerDocument(opt =>
            {
                opt.AutoTagPathSegmentIndex = 0;

                opt.FlattenSchema = true;
            });
        }

        private void ConfigureFastEndpoint(IServiceCollection services)
        {
            var assemblies = EndpointsAssemblyRegistry.GetAssemblies();

            services.AddFastEndpoints(opt =>
            {
                opt.Assemblies = assemblies;

            });
        }

        private void ConfigureControllers(IServiceCollection services)
        {
            services.AddControllers();
        }

    }
}
