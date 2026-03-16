using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Domain.Repositories;
using Invoria.Catalog.Application.Products.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IApplicatonRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly ICatalogRepository<Product> _productRepository;
        private readonly IProductResponseFactory _productResponseFactory;

        public CreateProductCommandHandler(ICatalogRepository<Product> productRepository, IProductResponseFactory productResponseFactory)
        {
            _productRepository = productRepository;
            _productResponseFactory = productResponseFactory;
        }

        public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product(request.Name, request.Code, request.Price);

            await _productRepository.Add(product);

            var dto = await _productResponseFactory.PrepareDto(product);

            return dto;
        }
    }
}
