using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Catalog.Contracts.Dtos;

namespace Invoria.Catalog.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IQuery<ProductDto>
    {
        public string Id { get; set; } = string.Empty;
    }
}
