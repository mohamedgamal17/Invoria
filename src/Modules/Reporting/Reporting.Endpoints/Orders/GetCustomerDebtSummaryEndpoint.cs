using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Reporting.Application.Orders.Queries.GetCustomerDebtSummary;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Reporting.Endpoints.Orders;

public sealed class GetCustomerDebtSummaryEndpoint
    : EndpointBase<GetCustomerDebtSummaryRequest, CustomerDebtOverviewDto>
{
    private readonly IMediator _mediator;

    public GetCustomerDebtSummaryEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("orders/customer-debt-overview/{customerId}");
        AllowAnonymous();

        Group<ReportingRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get customer debt overview";
            s.Description =
                "Returns the materialized debt overview for a single customer from completed reported orders with outstanding balance. " +
                "Data may lag live orders by about 5 minutes. Returns 404 when the customer has no debt summary row.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the customer debt overview snapshot.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetCustomerDebtSummaryRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var result = await _mediator.Send(
            new GetCustomerDebtSummaryQuery { CustomerId = req.CustomerId },
            ct);

        await SendResultAsync(result, ct);
    }
}
