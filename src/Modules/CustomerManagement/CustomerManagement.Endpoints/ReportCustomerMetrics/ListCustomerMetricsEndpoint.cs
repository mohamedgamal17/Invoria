using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.CustomerManagement.Application.ReportCustomerMetrics.Queries.ListCustomerMetrics;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Endpoints.ReportCustomerMetrics.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.CustomerManagement.Endpoints.ReportCustomerMetrics
{
    public class ListCustomerMetricsEndpoint : EndpointBase<ListCustomerMetricsRequest, PagingDto<ReportCustomerMetricsPeriodDto>>
    {
        private readonly IMediator _mediator;

        public ListCustomerMetricsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
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
                s.Summary = "List customer creation metrics per period";
                s.Description = "Returns a paged list of customer creation metrics for a given period, ordered by date descending.";
                s.Responses[StatusCodes.Status200OK] =
                    InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged customer metrics data.";
                s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
                s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
            });
        }

        public override async Task HandleAsync(ListCustomerMetricsRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var query = new ListCustomerMetricsQuery
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
