using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Procurement.Endpoints.ReportPurchaseOrdersCompletedMetrics.Requests;

public class ListPurchaseOrdersCompletedMetricsRequest : PagingParams
{
    [QueryParam]
    public ReportPeriod Period { get; set; }
}

public class ListPurchaseOrdersCompletedMetricsRequestValidator : AbstractValidator<ListPurchaseOrdersCompletedMetricsRequest>
{
    public ListPurchaseOrdersCompletedMetricsRequestValidator()
    {
        Include(new PagingParamasValidator<ListPurchaseOrdersCompletedMetricsRequest>());

        RuleFor(x => x.Period)
            .IsInEnum();
    }
}