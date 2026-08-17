using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Queries.ListOrderSalesProfitMetrics;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Endpoints.ReportOrderSalesProfitMetrics.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Ordering.Endpoints.ReportOrderSalesProfitMetrics;

public class ListOrderSalesProfitMetricsEndpoint : EndpointBase<ListOrderSalesProfitMetricsRequest, PagingDto<ReportOrderSalesProfitMetricsPeriodDto>>
{
    private readonly IMediator _mediator;

    public ListOrderSalesProfitMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("metrics");
        AllowAnonymous();

        Group<ReportRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List order sales profit metrics per period";
            s.Description = "Returns a paged list of order sales profit metrics for a given period, ordered by date descending.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged order sales profit metrics data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListOrderSalesProfitMetricsRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListOrderSalesProfitMetricsQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            Period = req.Period
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}