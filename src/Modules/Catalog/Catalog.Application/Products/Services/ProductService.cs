using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Catalog.Application.Products.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Catalog.Application.Products.Services
{
    public class ProductService : IProductService
    {
        private readonly ICatalogRepository<Product> _productRepository;
        private readonly IProductResponseFactory _productResponseFactory;

        public ProductService(
            ICatalogRepository<Product> productRepository,
            IProductResponseFactory productResponseFactory)
        {
            _productRepository = productRepository;
            _productResponseFactory = productResponseFactory;
        }

        public async Task<Result<ProductDto>> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.SingleOrDefault(p => p.Id == id, cancellationToken);
            if (product == null)
            {
                return Result.Failure<ProductDto>(new NotFoundException($"Product with ID {id} not found"));
            }

            var dto = await _productResponseFactory.PrepareDto(product);

            return dto;
        }

        public async Task<Result<IReadOnlyList<ProductDto>>> ListProductsByIdsAsync(
            IEnumerable<string> ids,
            CancellationToken cancellationToken = default)
        {
            var idList = ids
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (idList.Count == 0)
            {
                return new List<ProductDto>();
            }

            var products = await _productRepository
                .AsQuerable()
                .Where(p => idList.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var dtos = await _productResponseFactory.PrepareListDto(products);

            return dtos;
        }
    }
}
