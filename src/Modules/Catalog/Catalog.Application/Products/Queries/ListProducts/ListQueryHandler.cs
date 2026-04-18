using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Catalog.Application.Products.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.Products.Queries.ListProducts
{
    public class ListQueryHandler : IApplicatonRequestHandler<ListProductQuery, PagingDto<ProductDto>>
    {

        private readonly ICatalogRepository<Product> _productRepository;
        private readonly IProductResponseFactory _productResponseFactory;

        public ListQueryHandler(ICatalogRepository<Product> productRepository, IProductResponseFactory productResponseFactory)
        {
            _productRepository = productRepository;
            _productResponseFactory = productResponseFactory;
        }

        public async Task<Result<PagingDto<ProductDto>>> Handle(ListProductQuery request, CancellationToken cancellationToken)
        {
            var query = _productRepository.AsQuerable();

            var nameTerm = request.Name?.Trim();
            if (!string.IsNullOrEmpty(nameTerm))
            {
                var normalizedNameTerm = nameTerm.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(normalizedNameTerm));
            }

            var result = await query.ToPaged(request.Skip, request.Length);

            var response = await _productResponseFactory.PreparePagingDto(result);

            return response;
        }
    }
}
