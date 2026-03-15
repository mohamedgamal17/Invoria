using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Modules.Catalog.Contracts.Dtos;
using Invoria.Modules.Catalog.Domain.Products;

namespace Invoria.Modules.Catalog.Application.Products.Factories
{
    public interface IProductResponseFactory : IResponseFactory<Product , ProductDto>
    { 

    }
}
