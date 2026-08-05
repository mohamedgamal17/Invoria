using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Catalog.Endpoints.ReportProductMetrics.Requests
{
    public class ListProductMetricsRequest : PagingParams
    {
        [QueryParam]
        public ReportPeriod Period { get; set; }
    }

    public class ListProductMetricsRequestValidator : AbstractValidator<ListProductMetricsRequest>
    {
        public ListProductMetricsRequestValidator()
        {
            Include(new PagingParamasValidator<ListProductMetricsRequest>());

            RuleFor(x => x.Period)
                .IsInEnum();
        }
    }
}