using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
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
        }

        public override async Task HandleAsync(ListCustomersRequest req, CancellationToken ct)
        {
            var validator = Resolve<IValidator<ListCustomersRequest>>();

            var validationResult = validator.Validate(req);

            if (!validationResult.IsValid)
            {
                return;
            }

            var query = new ListCustomerQuery
            {
                Skip = req.Skip,
                Length = req.Length
            };

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}

