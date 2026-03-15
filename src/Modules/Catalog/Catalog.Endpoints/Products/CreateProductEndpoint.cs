using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Modules.Catalog.Application.Products.Commands.CreateProduct;
using Invoria.Modules.Catalog.Contracts.Dtos;
using Invoria.Modules.Catalog.Endpoints.Products.Requests;
using MediatR;
namespace Invoria.Modules.Catalog.Endpoints.Products
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

        }

        public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
        {
            var validator = Resolve<IValidator<ProductRequest>>();

            var validationResult = validator.Validate(req);

            if (!validationResult.IsValid)
            {
                return;
            }

            var command = new CreateProductCommand(req.Name, req.Code, req.Price);

            var result = await _mediator.Send(command);

           await  SendResultAsync(result);
        }
    }
}
