using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
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

            Summary(s =>
            {
                s.Summary = "Get customer by id";
                s.Description = "Returns a customer by identifier.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the customer DTO.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(GetCustomerByIdRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var query = new GetCustomerByIdQuery
            {
                Id = req.Id
            };

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}

