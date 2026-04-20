using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Catalog.Application.Products.Commands.CreateProduct;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Infrastructure;
using Invoria.Inventory.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Application.Tests
{
    public class CatalogTestModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallModule<CatalogModuleInstaller>(configuration);
            services.InstallModule<InventoryModuleInstaller>(configuration);
        }

    }
}
