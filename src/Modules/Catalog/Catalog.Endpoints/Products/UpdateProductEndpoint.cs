using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Catalog.Application.Products.Commands.UpdateProduct;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Endpoints.Products.Requests;
using MediatR;

namespace Invoria.Catalog.Endpoints.Products
{
    public class UpdateProductEndpoint : EndpointBase<UpdateProductRequest, ProductDto>
    {
        private readonly IMediator _mediator;

        public UpdateProductEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
            : base(resultMapper)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("{id}");
            AllowAnonymous();

            Group<ProductRoutingGroup>();

            Summary(s =>
            {
                s.Summary = "Update product";
                s.Description = "Updates an existing product by identifier.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated product.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
                s.Responses[StatusCodes.Status409Conflict] = InvoriaOpenApiResponseDescriptions.Conflict409;
                s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var command = new UpdateProductCommand(req.Id, req.Name, req.Price);

            var result = await _mediator.Send(command, ct);

            await SendResultAsync(result);
        }
    }
}
