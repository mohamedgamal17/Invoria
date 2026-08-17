using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Endpoints.ReportOrderSalesMetrics.Requests;

public class ListOrderSalesMetricsRequest : PagingParams
{
    [QueryParam]
    public ReportPeriod Period { get; set; }
}

public class ListOrderSalesMetricsRequestValidator : AbstractValidator<ListOrderSalesMetricsRequest>
{
    public ListOrderSalesMetricsRequestValidator()
    {
        Include(new PagingParamasValidator<ListOrderSalesMetricsRequest>());

        RuleFor(x => x.Period)
            .IsInEnum();
    }
}