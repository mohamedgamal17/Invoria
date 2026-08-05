using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Catalog.Application.ReportProductMetrics.Queries.GetProductMetrics;
using Invoria.Catalog.Contracts.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Catalog.Endpoints.ReportProductMetrics
{
    public class GetProductMetricsEndpoint : EndpointBaseWithoutRequest<ReportProductMetricsDto>
    {
        private readonly IMediator _mediator;

        public GetProductMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
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
                s.Summary = "Get product creation metrics overview";
                s.Description = "Returns current day, month, year and all-time product creation metrics.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the report product metrics overview.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var query = new GetProductMetricsQuery();

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}