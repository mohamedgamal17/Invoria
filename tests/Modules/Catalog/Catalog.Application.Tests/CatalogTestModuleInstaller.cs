using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Modules.Catalog.Application.Products.Commands.CreateProduct;
using Invoria.Modules.Catalog.Contracts.Dtos;
using Invoria.Modules.Catalog.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Modules.Catalog.Application.Tests
{
    public class CatalogTestModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallModule<CatalogModuleInstaller>(configuration);

        }

    }
}
