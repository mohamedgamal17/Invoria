using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.Products.Factories
{
    public interface IProductResponseFactory : IResponseFactory<Product , ProductDto>
    { 

    }
}
