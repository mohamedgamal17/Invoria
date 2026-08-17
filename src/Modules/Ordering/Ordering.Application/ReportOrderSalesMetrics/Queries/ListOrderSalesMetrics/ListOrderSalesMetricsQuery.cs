using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Application.ReportOrderSalesMetrics.Queries.ListOrderSalesMetrics;

public class ListOrderSalesMetricsQuery : PagingParams, IQuery<PagingDto<ReportOrderSalesMetricsPeriodDto>>
{
    public ReportPeriod Period { get; set; }
}