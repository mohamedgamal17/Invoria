using FluentValidation;
using FastEndpoints;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Endpoints.Products.Requests
{
    public class ListProductsRequest : PagingParams
    {
        [QueryParam]
        public string? Name { get; set; }
    }

    public class ListProductsRequestValidator : AbstractValidator<ListProductsRequest>
    {
        public ListProductsRequestValidator()
        {
            Include(new PagingParamasValidator<ListProductsRequest>());

            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name!)
                    .MaximumLength(ProductTableConsts.NameMaxLength);
            });
        }
    }
}
