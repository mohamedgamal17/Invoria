using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Reporting.Application.Orders.Queries.GetOrderStatusSummary;
using Invoria.Reporting.Contracts.Dtos;
using Invoria.Reporting.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Reporting.Endpoints.Orders;

public sealed class GetOrderStatusSummaryEndpoint : EndpointBase<GetOrderStatusSummaryRequest, IReadOnlyList<OrderStatusSummaryItemDto>>
{
    private readonly IMediator _mediator;

    public GetOrderStatusSummaryEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("orders/status-summary");
        AllowAnonymous();

        Group<ReportingRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Order status summary";
            s.Description =
                "Returns counts of reported orders grouped by current order status. " +
                "Optional FromUtc and ToUtc: when both are set, only orders whose CreatedAt (UTC calendar day) falls in that inclusive range are included; " +
                "when both are omitted, all materialized summary data is included (all time). " +
                "Values come from a materialized table refreshed about every 5 minutes and may lag live data.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns status summary rows.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetOrderStatusSummaryRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var result = await _mediator.Send(
            new GetOrderStatusSummaryQuery
            {
                FromUtc = req.FromUtc,
                ToUtc = req.ToUtc
            },
            ct);

        await SendResultAsync(result, ct);
    }
}
