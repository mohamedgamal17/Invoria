using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.CustomerManagement.Application.Customers.Commands.UpdateCustomer;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Endpoints.Customers.Requests;
using MediatR;

namespace Invoria.CustomerManagement.Endpoints.Customers
{
    public class UpdateCustomerEndpoint : EndpointBase<UpdateCustomerRequest, CustomerDto>
    {
        private readonly IMediator _mediator;

        public UpdateCustomerEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
            : base(resultMapper)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("{id}");
            AllowAnonymous();
            Group<CustomerRoutingGroup>();
        }

        public override async Task HandleAsync(UpdateCustomerRequest req, CancellationToken ct)
        {
            var validator = Resolve<IValidator<UpdateCustomerRequest>>();

            var validationResult = validator.Validate(req);

            if (!validationResult.IsValid)
            {
                return;
            }

            var command = new UpdateCustomerCommand(req.Id, req.Name);

            var result = await _mediator.Send(command, ct);

            await SendResultAsync(result, ct);
        }
    }
}

