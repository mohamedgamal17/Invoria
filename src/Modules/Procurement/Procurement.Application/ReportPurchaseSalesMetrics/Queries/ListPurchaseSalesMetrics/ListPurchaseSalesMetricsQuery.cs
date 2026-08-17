using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Queries.ListPurchaseSalesMetrics;

public class ListPurchaseSalesMetricsQuery : PagingParams, IQuery<PagingDto<ReportPurchaseSalesMetricsPeriodDto>>
{
    public ReportPeriod Period { get; set; }
}