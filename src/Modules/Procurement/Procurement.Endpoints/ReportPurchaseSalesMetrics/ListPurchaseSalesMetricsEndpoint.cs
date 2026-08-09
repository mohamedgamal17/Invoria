using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Queries.ListPurchaseSalesMetrics;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.ReportPurchaseSalesMetrics.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Procurement.Endpoints.ReportPurchaseSalesMetrics;

public sealed class ListPurchaseSalesMetricsEndpoint : EndpointBase<ListPurchaseSalesMetricsRequest, PagingDto<ReportPurchaseSalesMetricsPeriodDto>>
{
    private readonly IMediator _mediator;

    public ListPurchaseSalesMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
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
            s.Summary = "List purchase sales metrics per period";
            s.Description = "Returns a paged list of purchase sales metrics for a given period, ordered by date descending.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged purchase sales metrics data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListPurchaseSalesMetricsRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListPurchaseSalesMetricsQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            Period = req.Period
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}