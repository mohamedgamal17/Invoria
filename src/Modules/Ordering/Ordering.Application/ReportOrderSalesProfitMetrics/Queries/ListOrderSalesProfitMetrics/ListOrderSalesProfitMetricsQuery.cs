using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Queries.ListOrderSalesProfitMetrics;

public class ListOrderSalesProfitMetricsQuery : PagingParams, IQuery<PagingDto<ReportOrderSalesProfitMetricsPeriodDto>>
{
    public ReportPeriod Period { get; set; }
}