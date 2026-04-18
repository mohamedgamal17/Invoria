using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Catalog.Application.Products.Commands.CreateProduct;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Endpoints.Products.Requests;
using MediatR;
namespace Invoria.Catalog.Endpoints.Products
{
    public class CreateProductEndpoint : EndpointBase<CreateProductRequest, ProductDto>
    {
        private readonly IMediator _mediator;

        public CreateProductEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
            : base(resultMapper)
        {
            _mediator = mediator;
        }


        public override void Configure()
        {
            Post("");
            AllowAnonymous();

            Group<ProductRoutingGroup>();

            Summary(s =>
            {
                s.Summary = "Create product";
                s.Description = "Creates a new catalog product.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the created product.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var command = new CreateProductCommand(req.Name, req.Code, req.Price);

            var result = await _mediator.Send(command);

           await  SendResultAsync(result);
        }
    }
}
