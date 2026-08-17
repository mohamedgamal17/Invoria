using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Endpoints.ReportOrderSalesProfitMetrics.Requests;

public class ListOrderSalesProfitMetricsRequest : PagingParams
{
    [QueryParam]
    public ReportPeriod Period { get; set; }
}

public class ListOrderSalesProfitMetricsRequestValidator : AbstractValidator<ListOrderSalesProfitMetricsRequest>
{
    public ListOrderSalesProfitMetricsRequestValidator()
    {
        Include(new PagingParamasValidator<ListOrderSalesProfitMetricsRequest>());

        RuleFor(x => x.Period)
            .IsInEnum();
    }
}