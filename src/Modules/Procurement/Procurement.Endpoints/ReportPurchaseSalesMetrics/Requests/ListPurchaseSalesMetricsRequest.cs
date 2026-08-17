using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Procurement.Endpoints.ReportPurchaseSalesMetrics.Requests;

public class ListPurchaseSalesMetricsRequest : PagingParams
{
    [QueryParam]
    public ReportPeriod Period { get; set; }
}

public class ListPurchaseSalesMetricsRequestValidator : AbstractValidator<ListPurchaseSalesMetricsRequest>
{
    public ListPurchaseSalesMetricsRequestValidator()
    {
        Include(new PagingParamasValidator<ListPurchaseSalesMetricsRequest>());

        RuleFor(x => x.Period)
            .IsInEnum();
    }
}