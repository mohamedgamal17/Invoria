using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderCompletedMetrics.Factories;

using ReportOrderCompletedMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderCompletedMetrics;

public interface IReportOrderCompletedMetricsResponseFactory : IResponseFactory<ReportOrderCompletedMetricsEntity, ReportOrderCompletedMetricsPeriodDto>
{
    Task<ReportOrderCompletedMetricsDto> PrepareMetricsDto(
        ReportOrderCompletedMetricsEntity daily,
        ReportOrderCompletedMetricsEntity monthly,
        ReportOrderCompletedMetricsEntity yearly,
        ReportOrderCompletedMetricsEntity allTime);
}