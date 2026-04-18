using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Catalog.Contracts.Dtos;

namespace Invoria.Catalog.Application.Products.Queries.ListProducts
{
    public class ListProductQuery  : PagingParams , IQuery<PagingDto<ProductDto>>
    {
        public string? Name { get; set; }
    }
}
