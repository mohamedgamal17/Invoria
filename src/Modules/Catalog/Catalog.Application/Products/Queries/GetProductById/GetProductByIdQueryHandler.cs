using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Domain.Repositories;
using Invoria.Catalog.Application.Products.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IApplicatonRequestHandler<GetProductByIdQuery, ProductDto>
    {
        private readonly ICatalogRepository<Product> _productRepository;
        private readonly IProductResponseFactory _productResponseFactory;

        public GetProductByIdQueryHandler(ICatalogRepository<Product> productRepository, IProductResponseFactory productResponseFactory)
        {
            _productRepository = productRepository;
            _productResponseFactory = productResponseFactory;
        }

        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.SingleOrDefault(p => p.Id == request.Id, cancellationToken);
            if (product == null)
            {
                return Result.Failure<ProductDto>(new NotFoundException($"Product with ID {request.Id} not found"));
            }

            var dto = await _productResponseFactory.PrepareDto(product);

            return dto;
        }
    }
}
