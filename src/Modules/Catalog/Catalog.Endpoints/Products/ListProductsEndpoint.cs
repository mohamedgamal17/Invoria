using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
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

            Summary(s =>
            {
                s.Summary = "List products";
                s.Description = "Returns a paged list of catalog products with optional name filtering.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged product data.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(ListProductsRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var query = new ListProductQuery
            {
                Skip = req.Skip,
                Length = req.Length,
                Name = req.Name
            };

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}
