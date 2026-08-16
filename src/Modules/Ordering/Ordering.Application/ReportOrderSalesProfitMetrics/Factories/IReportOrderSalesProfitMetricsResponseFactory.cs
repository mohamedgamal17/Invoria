using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Factories;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

public interface IReportOrderSalesProfitMetricsResponseFactory : IResponseFactory<ReportOrderSalesProfitMetricsEntity, ReportOrderSalesProfitMetricsPeriodDto>
{
    Task<ReportOrderSalesProfitMetricsDto> PrepareMetricsDto(
        ReportOrderSalesProfitMetricsEntity daily,
        ReportOrderSalesProfitMetricsEntity monthly,
        ReportOrderSalesProfitMetricsEntity yearly,
        ReportOrderSalesProfitMetricsEntity allTime);
}