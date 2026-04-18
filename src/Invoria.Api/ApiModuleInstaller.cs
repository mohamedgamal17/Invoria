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
using Invoria.Procurement.Infrastructure;
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
            services.InstallModule<ProcurementModuleInstaller>(configuration);

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

                opt.DocumentSettings = s =>
                {
                    s.Title = "Invoria API";
                    s.Version = "v1";
                    s.Description =
                        "HTTP API for the Invoria modular backend. The host wires Catalog, CustomerManagement, Ordering, Inventory, and Procurement. "
                        + "Successful and failed responses are wrapped in a JSON envelope: on success, isSuccess is true and the payload is in result; "
                        + "on failure, isSuccess is false and problem details are in error (RFC 7807-style fields plus optional errorCode and field errors for validation).";
                };

                opt.TagDescriptions = tags =>
                {
                    tags["Products"] =
                        "Catalog products: create, update, list, and retrieve items sold through Invoria.";
                    tags["Customers"] =
                        "Customer master data: register, update, search, and retrieve customers.";
                    tags["Batches"] =
                        "Inventory batches: create, update, list, and retrieve stock batches.";
                    tags["Orders"] =
                        "Sales orders: create and manage order lifecycle (accept, dispatch, complete, cancel, and related transitions).";
                    tags["Suppliers"] =
                        "Procurement suppliers: create, update, list, and retrieve supplier records.";
                    tags["PurchaseOrders"] =
                        "Purchase orders: create and drive PO workflow (submit, approve, reject, complete, cancel, reopen, and related actions).";
                };
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
