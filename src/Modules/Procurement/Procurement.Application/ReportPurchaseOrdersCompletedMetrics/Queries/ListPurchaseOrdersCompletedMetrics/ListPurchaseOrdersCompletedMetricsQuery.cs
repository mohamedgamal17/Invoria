using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Queries.ListPurchaseOrdersCompletedMetrics;

public class ListPurchaseOrdersCompletedMetricsQuery : PagingParams, IQuery<PagingDto<ReportPurchaseOrdersCompletedMetricsPeriodDto>>
{
    public ReportPeriod Period { get; set; }
}