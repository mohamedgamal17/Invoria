using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderSalesMetrics.Factories;

using ReportOrderSalesMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesMetrics;

public interface IReportOrderSalesMetricsResponseFactory : IResponseFactory<ReportOrderSalesMetricsEntity, ReportOrderSalesMetricsPeriodDto>
{
    Task<ReportOrderSalesMetricsDto> PrepareMetricsDto(
        ReportOrderSalesMetricsEntity daily,
        ReportOrderSalesMetricsEntity monthly,
        ReportOrderSalesMetricsEntity yearly,
        ReportOrderSalesMetricsEntity allTime);
}