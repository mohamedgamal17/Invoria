using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;
using Invoria.Inventory.Application.Stock;
using Invoria.Inventory.Contracts.Dtos;

namespace Invoria.Catalog.Application.Products.Factories
{
    public class ProductResposneFactory : ResponseFactory<Product, ProductDto>, IProductResponseFactory
    {
        private readonly IProductStockService _productStockService;

        public ProductResposneFactory(IProductStockService productStockService)
        {
            _productStockService = productStockService;
        }

        public override async Task<ProductDto> PrepareDto(Product view)
        {
            var stock = await _productStockService.GetProductStock(view.Id).ConfigureAwait(false);
            return CreateProductDto(view, stock);
        }

        public override async Task<List<ProductDto>> PrepareListDto(List<Product> views)
        {
            if (views.Count == 0)
            {
                return new List<ProductDto>();
            }

            var productIds = views.Select(x => x.Id).Distinct().ToList();
            var stockMap = await _productStockService.GetListProductsStock(productIds).ConfigureAwait(false);

            var dtos = new List<ProductDto>(views.Count);
            foreach (var view in views)
            {
                var stock = stockMap.TryGetValue(view.Id, out var mappedStock)
                    ? mappedStock
                    : new StockDto();
                dtos.Add(CreateProductDto(view, stock));
            }

            return dtos;
        }

        private ProductDto CreateProductDto(Product view, StockDto stock)
        {
            var dto = new ProductDto
            {
                Id = view.Id,
                Name = view.Name,
                Code = view.Code,
                Price = view.Price,
                Stock = stock
            };

            MapAudited(view, dto);
            return dto;
        }
    }
}
