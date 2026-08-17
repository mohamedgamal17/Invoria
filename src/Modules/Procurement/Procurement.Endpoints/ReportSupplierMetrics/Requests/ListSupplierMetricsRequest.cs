using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Procurement.Endpoints.ReportSupplierMetrics.Requests;

public class ListSupplierMetricsRequest : PagingParams
{
    [QueryParam]
    public ReportPeriod Period { get; set; }
}

public class ListSupplierMetricsRequestValidator : AbstractValidator<ListSupplierMetricsRequest>
{
    public ListSupplierMetricsRequestValidator()
    {
        Include(new PagingParamasValidator<ListSupplierMetricsRequest>());

        RuleFor(x => x.Period)
            .IsInEnum();
    }
}