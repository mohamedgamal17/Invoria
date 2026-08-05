using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Catalog.Application.ReportProductMetrics.Queries.ListProductMetrics;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Endpoints.ReportProductMetrics.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Catalog.Endpoints.ReportProductMetrics
{
    public class ListProductMetricsEndpoint : EndpointBase<ListProductMetricsRequest, PagingDto<ReportProductMetricsPeriodDto>>
    {
        private readonly IMediator _mediator;

        public ListProductMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
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
                s.Summary = "List product creation metrics per period";
                s.Description = "Returns a paged list of product creation metrics for a given period, ordered by date descending.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged product metrics data.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(ListProductMetricsRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var query = new ListProductMetricsQuery
            {
                Skip = req.Skip,
                Length = req.Length,
                Period = req.Period
            };

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}