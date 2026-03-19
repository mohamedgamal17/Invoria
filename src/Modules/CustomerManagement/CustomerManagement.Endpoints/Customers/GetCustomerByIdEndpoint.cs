using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.CustomerManagement.Application.Customers.Queries.GetCustomerById;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Endpoints.Customers.Requests;
using MediatR;

namespace Invoria.CustomerManagement.Endpoints.Customers
{
    public class GetCustomerByIdEndpoint : EndpointBase<GetCustomerByIdRequest, CustomerDto>
    {
        private readonly IMediator _mediator;

        public GetCustomerByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
            : base(resultMapper)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("{id}");
            AllowAnonymous();

            Group<CustomerRoutingGroup>();
        }

        public override async Task HandleAsync(GetCustomerByIdRequest req, CancellationToken ct)
        {
            var validator = Resolve<IValidator<GetCustomerByIdRequest>>();

            var validationResult = validator.Validate(req);

            if (!validationResult.IsValid)
            {
                return;
            }

            var query = new GetCustomerByIdQuery
            {
                Id = req.Id
            };

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}

