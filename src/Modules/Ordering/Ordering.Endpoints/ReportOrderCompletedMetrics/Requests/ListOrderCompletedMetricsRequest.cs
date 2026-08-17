using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Endpoints.ReportOrderCompletedMetrics.Requests;

public class ListOrderCompletedMetricsRequest : PagingParams
{
    [QueryParam]
    public ReportPeriod Period { get; set; }
}

public class ListOrderCompletedMetricsRequestValidator : AbstractValidator<ListOrderCompletedMetricsRequest>
{
    public ListOrderCompletedMetricsRequestValidator()
    {
        Include(new PagingParamasValidator<ListOrderCompletedMetricsRequest>());

        RuleFor(x => x.Period)
            .IsInEnum();
    }
}