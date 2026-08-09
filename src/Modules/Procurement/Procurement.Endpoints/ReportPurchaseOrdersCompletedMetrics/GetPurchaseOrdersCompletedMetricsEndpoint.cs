using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Queries.GetPurchaseOrdersCompletedMetrics;
using Invoria.Procurement.Contracts.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Procurement.Endpoints.ReportPurchaseOrdersCompletedMetrics;

public sealed class GetPurchaseOrdersCompletedMetricsEndpoint : EndpointBaseWithoutRequest<ReportPurchaseOrdersCompletedMetricsDto>
{
    private readonly IMediator _mediator;

    public GetPurchaseOrdersCompletedMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("overview");
        AllowAnonymous();

        Group<ReportRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get purchase orders completion metrics overview";
            s.Description = "Returns current day, month, year and all-time purchase orders completion metrics.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the report purchase orders completed metrics overview.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetPurchaseOrdersCompletedMetricsQuery();

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}