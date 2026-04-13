using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Catalog.Application.Products.Queries.GetProductById;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Endpoints.Products.Requests;
using MediatR;

namespace Invoria.Catalog.Endpoints.Products
{
    public class GetProductByIdEndpoint : EndpointBase<GetProductByIdRequest, ProductDto>
    {
        private readonly IMediator _mediator;

        public GetProductByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
            : base(resultMapper)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("{id}");
            AllowAnonymous();

            Group<ProductRoutingGroup>();
        }

        public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var query = new GetProductByIdQuery
            {
                Id = req.Id
            };

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}
