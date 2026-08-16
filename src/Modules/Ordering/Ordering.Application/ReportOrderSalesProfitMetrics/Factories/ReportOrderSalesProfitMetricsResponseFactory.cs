using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Factories;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

public class ReportOrderSalesProfitMetricsResponseFactory : ResponseFactory<ReportOrderSalesProfitMetricsEntity, ReportOrderSalesProfitMetricsPeriodDto>, IReportOrderSalesProfitMetricsResponseFactory
{
    public override Task<ReportOrderSalesProfitMetricsPeriodDto> PrepareDto(ReportOrderSalesProfitMetricsEntity view)
    {
        var dto = new ReportOrderSalesProfitMetricsPeriodDto
        {
            Date = view.Date,
            TotalRevenue = view.TotalRevenue,
            TotalCost = view.TotalCost,
            TotalProfit = view.TotalProfit,
            TotalReturnAmount = view.TotalReturnAmount,
            Period = view.Period
        };

        return Task.FromResult(dto);
    }

    public async Task<ReportOrderSalesProfitMetricsDto> PrepareMetricsDto(
        ReportOrderSalesProfitMetricsEntity daily,
        ReportOrderSalesProfitMetricsEntity monthly,
        ReportOrderSalesProfitMetricsEntity yearly,
        ReportOrderSalesProfitMetricsEntity allTime)
    {
        var dto = new ReportOrderSalesProfitMetricsDto
        {
            ThisDay = await PrepareDto(daily),
            ThisMonth = await PrepareDto(monthly),
            ThisYear = await PrepareDto(yearly),
            AllTime = await PrepareDto(allTime)
        };

        return dto;
    }
}