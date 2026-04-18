using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.CustomerManagement.Application.Customers.Queries.ListCustomers;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Endpoints.Customers.Requests;
using MediatR;

namespace Invoria.CustomerManagement.Endpoints.Customers
{
    public class ListCustomersEndpoint : EndpointBase<ListCustomersRequest, PagingDto<CustomerDto>>
    {
        private readonly IMediator _mediator;

        public ListCustomersEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
            : base(resultMapper)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("");
            AllowAnonymous();

            Group<CustomerRoutingGroup>();

            Summary(s =>
            {
                s.Summary = "List customers";
                s.Description = "Returns a paged list of customers.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged customer data.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(ListCustomersRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var query = new ListCustomerQuery
            {
                Skip = req.Skip,
                Length = req.Length,
                Name = req.Name
            };

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}

