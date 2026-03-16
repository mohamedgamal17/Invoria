using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.Products.Factories
{
    public class ProductResposneFactory : ResponseFactory<Product, ProductDto>, IProductResponseFactory
    {
        public override Task<ProductDto> PrepareDto(Product view)
        {
            var dto = new ProductDto
            {
                Id = view.Id,
                Name = view.Name,
                Code = view.Code,
                Price = view.Price
            };

            MapAudited(view, dto);

            return Task.FromResult(dto);
        }
    }
}
