using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Domain.Repositories;
using Invoria.Modules.Catalog.Application.Products.Factories;
using Invoria.Modules.Catalog.Contracts.Dtos;
using Invoria.Modules.Catalog.Domain;
using Invoria.Modules.Catalog.Domain.Products;

namespace Invoria.Modules.Catalog.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IApplicatonRequestHandler<UpdateProductCommand, ProductDto>
    {
        private readonly ICatalogRepository<Product> _productRepository;
        private readonly IProductResponseFactory _productResponseFactory;

        public UpdateProductCommandHandler(ICatalogRepository<Product> productRepository, IProductResponseFactory productResponseFactory)
        {
            _productRepository = productRepository;
            _productResponseFactory = productResponseFactory;
        }

        public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.SingleOrDefault(p => p.Id == request.Id, cancellationToken);
            if (product == null)
            {
                return Result.Failure<ProductDto>(new NotFoundException($"Product with ID {request.Id} not found"));
            }

            product.Update(request.Name, request.Code, request.Price);

            await _productRepository.Update(product, cancellationToken);

            var dto = await _productResponseFactory.PrepareDto(product);

            return dto;
        }
    }
}
