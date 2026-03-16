using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;

namespace Invoria.Catalog.Endpoints.Products.Requests
{
    public class ListProductsRequest : PagingParams
    {
    }

    public class ListProductsRequestValidator : AbstractValidator<ListProductsRequest>
    {
        public ListProductsRequestValidator()
        {
            Include(new PagingParamasValidator<ListProductsRequest>());
        }
    }
}
