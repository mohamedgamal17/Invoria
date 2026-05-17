using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Reporting.Application.Orders.Queries.ListOrderPeriodSummary;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Reporting.Endpoints.Orders;

public sealed class ListOrderPeriodSummaryEndpoint : EndpointBase<ListOrderPeriodSummaryRequest, PagingDto<OrderPeriodSummaryDto>>
{
    private readonly IMediator _mediator;

    public ListOrderPeriodSummaryEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("orders/period-summaries");
        AllowAnonymous();

        Group<ReportingRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List order period summaries";
            s.Description =
                "Returns a paginated slice of materialized period buckets from OrderPeriodSummary (global all reported orders). " +
                "Buckets use UTC placed date (CreatedAt) only. From/To are optional: when both omitted, the range is the last 30 UTC calendar days ending today; " +
                "when only To is omitted, From is required as the start and To defaults to today; when only From is omitted, To is taken from the request and From defaults to To minus 30 days. " +
                "GroupedBy selects Day (default), Week, or Month. Rows are ordered newest period first. Skip defaults to 0 and Length to 10. " +
                "This endpoint does not apply customer or status filters.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged period summaries as data/info (same shape as other list endpoints).";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListOrderPeriodSummaryRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var result = await _mediator.Send(
            new ListOrderPeriodSummaryQuery
            {
                Skip = req.Skip,
                Length = req.Length,
                From = req.From,
                To = req.To,
                GroupedBy = req.GroupedBy
            },
            ct);

        await SendResultAsync(result, ct);
    }
}
