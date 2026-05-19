using FastEndpoints;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Reporting.Application.Orders.Queries.GetDebtOverview;
using Invoria.Reporting.Contracts.Orders.Reports;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Reporting.Endpoints.Orders;

public sealed class GetDebtOverviewEndpoint : EndpointBase<EmptyRequest, DebtOverviewDto>
{
    private readonly IMediator _mediator;

    public GetDebtOverviewEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("orders/debt-overview");
        AllowAnonymous();

        Group<ReportingRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get debt overview";
            s.Description =
                "Returns the materialized global debt overview from completed reported orders with outstanding balance. " +
                "Values are rebuilt about every 5 minutes and may lag live order data. " +
                "This endpoint does not apply customer or date filters.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the global debt overview snapshot.";
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDebtOverviewQuery(), ct);

        await SendResultAsync(result, ct);
    }
}
