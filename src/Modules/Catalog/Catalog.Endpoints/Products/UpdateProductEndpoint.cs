using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
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
        }

        public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
        {
            var validator = Resolve<IValidator<UpdateProductRequest>>();

            var validationResult = validator.Validate(req);

            if (!validationResult.IsValid)
            {
                return;
            }

            var command = new UpdateProductCommand(req.Id, req.Name, req.Code, req.Price);

            var result = await _mediator.Send(command, ct);

            await SendResultAsync(result);
        }
    }
}
