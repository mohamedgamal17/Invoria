using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.CustomerManagement.Application.Customers.Commands.CreateCustomer;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Endpoints.Customers.Requests;
using MediatR;

namespace Invoria.CustomerManagement.Endpoints.Customers
{
    public class CreateCustomerEndpoint : EndpointBase<CreateCustomerRequest, CustomerDto>
    {
        private readonly IMediator _mediator;

        public CreateCustomerEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
            : base(resultMapper)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("");
            AllowAnonymous();

            Group<CustomerRoutingGroup>();
        }

        public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
        {
            var validator = Resolve<IValidator<CreateCustomerRequest>>();

            var validationResult = validator.Validate(req);

            if (!validationResult.IsValid)
            {
                return;
            }

            var command = new CreateCustomerCommand(req.Name);

            var result = await _mediator.Send(command, ct);

            await SendResultAsync(result, ct);
        }
    }
}

