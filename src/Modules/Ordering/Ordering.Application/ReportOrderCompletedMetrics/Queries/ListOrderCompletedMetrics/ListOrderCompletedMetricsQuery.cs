using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Application.ReportOrderCompletedMetrics.Queries.ListOrderCompletedMetrics;

public class ListOrderCompletedMetricsQuery : PagingParams, IQuery<PagingDto<ReportOrderCompletedMetricsPeriodDto>>
{
    public ReportPeriod Period { get; set; }
}