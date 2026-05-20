using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Reporting.Application.Orders.Queries.ListCustomerDebtOverview;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Reporting.Endpoints.Orders;

public sealed class ListCustomerDebtOverviewEndpoint
    : EndpointBase<ListCustomerDebtOverviewRequest, PagingDto<CustomerDebtOverviewDto>>
{
    private readonly IMediator _mediator;

    public ListCustomerDebtOverviewEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("orders/customer-debt-overview");
        AllowAnonymous();

        Group<ReportingRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List customer debt overview";
            s.Description =
                "Returns a paginated slice of per-customer debt overview rows from the materialized DebtSummary table " +
                "(completed orders with outstanding balance). Rows are ordered by highest total outstanding first, then customer id. " +
                "Skip defaults to 0 and Length to 10. Data may lag live orders by about 5 minutes.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged customer debt overview rows as data/info.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListCustomerDebtOverviewRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var result = await _mediator.Send(
            new ListCustomerDebtOverviewQuery
            {
                Skip = req.Skip,
                Length = req.Length
            },
            ct);

        await SendResultAsync(result, ct);
    }
}
