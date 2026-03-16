using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Catalog.Application.Products.Queries.ListProducts;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Endpoints.Products.Requests;
using MediatR;

namespace Invoria.Catalog.Endpoints.Products
{
    public class ListProductsEndpoint : EndpointBase<ListProductsRequest, PagingDto<ProductDto>>
    {
        private readonly IMediator _mediator;

        public ListProductsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
            : base(resultMapper)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("");
            AllowAnonymous();

            Group<ProductRoutingGroup>();
        }

        public override async Task HandleAsync(ListProductsRequest req, CancellationToken ct)
        {
            var validator = Resolve<IValidator<ListProductsRequest>>();

            var validationResult = validator.Validate(req);

            if (!validationResult.IsValid)
            {
                return;
            }

            var query = new ListProductQuery
            {
                Skip = req.Skip,
                Length = req.Length
            };

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}
