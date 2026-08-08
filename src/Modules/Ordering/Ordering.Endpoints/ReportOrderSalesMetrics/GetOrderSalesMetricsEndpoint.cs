using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.ReportOrderSalesMetrics.Queries.GetOrderSalesMetrics;
using Invoria.Ordering.Contracts.Orders.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Ordering.Endpoints.ReportOrderSalesMetrics;

public class GetOrderSalesMetricsEndpoint : EndpointBaseWithoutRequest<ReportOrderSalesMetricsDto>
{
    private readonly IMediator _mediator;

    public GetOrderSalesMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
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
            s.Summary = "Get order sales metrics overview";
            s.Description = "Returns current day, month, year and all-time order sales metrics.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the report order sales metrics overview.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetOrderSalesMetricsQuery();

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}