using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
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

            Summary(s =>
            {
                s.Summary = "Create customer";
                s.Description = "Registers a new customer.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the created customer.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var command = new CreateCustomerCommand(req.Name);

            var result = await _mediator.Send(command, ct);

            await SendResultAsync(result, ct);
        }
    }
}

