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

namespace Invoria.Api
{
    public class ApiModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallModule<CatalogModuleInstaller>(configuration)
                .AddApplicationInfrastructure();

            services.InstallModule<CustomerManagementModuleInstaller>(configuration);
            services.InstallModule<InventoryModuleInstaller>(configuration);
            services.InstallModule<OrderingModuleInstaller>(configuration);

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            ConfigureSwagger(services);

            ConfigureFastEndpoint(services);
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

    }
}
