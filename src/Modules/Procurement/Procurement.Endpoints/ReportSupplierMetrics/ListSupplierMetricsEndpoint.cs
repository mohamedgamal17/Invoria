using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.ReportSupplierMetrics.Queries.ListSupplierMetrics;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.ReportSupplierMetrics.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Procurement.Endpoints.ReportSupplierMetrics;

public sealed class ListSupplierMetricsEndpoint : EndpointBase<ListSupplierMetricsRequest, PagingDto<ReportSupplierMetricsPeriodDto>>
{
    private readonly IMediator _mediator;

    public ListSupplierMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
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
            s.Summary = "List supplier creation metrics per period";
            s.Description = "Returns a paged list of supplier creation metrics for a given period, ordered by date descending.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged supplier metrics data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListSupplierMetricsRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListSupplierMetricsQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            Period = req.Period
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}