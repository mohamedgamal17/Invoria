using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.CustomerManagement.Application.ReportCustomerMetrics.Queries.GetCustomerMetrics;
using Invoria.CustomerManagement.Contracts.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.CustomerManagement.Endpoints.ReportCustomerMetrics
{
    public class GetCustomerMetricsEndpoint : EndpointBaseWithoutRequest<ReportCustomerMetricsDto>
    {
        private readonly IMediator _mediator;

        public GetCustomerMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
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
                s.Summary = "Get customer creation metrics overview";
                s.Description = "Returns current day, month, year and all-time customer creation metrics.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the report customer metrics overview.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var query = new GetCustomerMetricsQuery();

            var result = await _mediator.Send(query, ct);

            await SendResultAsync(result, ct);
        }
    }
}
