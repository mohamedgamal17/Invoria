using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderSalesMetrics.Factories;

using ReportOrderSalesMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesMetrics;

public class ReportOrderSalesMetricsResponseFactory : ResponseFactory<ReportOrderSalesMetricsEntity, ReportOrderSalesMetricsPeriodDto>, IReportOrderSalesMetricsResponseFactory
{
    public override Task<ReportOrderSalesMetricsPeriodDto> PrepareDto(ReportOrderSalesMetricsEntity view)
    {
        var dto = new ReportOrderSalesMetricsPeriodDto
        {
            Date = view.Date,
            TotalAmount = view.TotalAmount,
            TotalNetAmount = view.TotalNetAmount,
            TotalReturnAmount = view.TotalReturnAmount,
            Period = view.Period
        };

        return Task.FromResult(dto);
    }

    public async Task<ReportOrderSalesMetricsDto> PrepareMetricsDto(
        ReportOrderSalesMetricsEntity daily,
        ReportOrderSalesMetricsEntity monthly,
        ReportOrderSalesMetricsEntity yearly,
        ReportOrderSalesMetricsEntity allTime)
    {
        var dto = new ReportOrderSalesMetricsDto
        {
            ThisDay = await PrepareDto(daily),
            ThisMonth = await PrepareDto(monthly),
            ThisYear = await PrepareDto(yearly),
            AllTime = await PrepareDto(allTime)
        };

        return dto;
    }
}