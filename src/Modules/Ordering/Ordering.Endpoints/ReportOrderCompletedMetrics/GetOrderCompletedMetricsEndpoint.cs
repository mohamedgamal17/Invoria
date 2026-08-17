using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.ReportOrderCompletedMetrics.Queries.GetOrderCompletedMetrics;
using Invoria.Ordering.Contracts.Orders.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Ordering.Endpoints.ReportOrderCompletedMetrics;

public class GetOrderCompletedMetricsEndpoint : EndpointBaseWithoutRequest<ReportOrderCompletedMetricsDto>
{
    private readonly IMediator _mediator;

    public GetOrderCompletedMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
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
            s.Summary = "Get order completion metrics overview";
            s.Description = "Returns current day, month, year and all-time order completion metrics.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the report order completion metrics overview.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetOrderCompletedMetricsQuery();

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}