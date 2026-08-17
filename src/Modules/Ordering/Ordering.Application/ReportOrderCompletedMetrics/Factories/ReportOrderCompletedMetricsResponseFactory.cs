using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderCompletedMetrics.Factories;

using ReportOrderCompletedMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderCompletedMetrics;

public class ReportOrderCompletedMetricsResponseFactory : ResponseFactory<ReportOrderCompletedMetricsEntity, ReportOrderCompletedMetricsPeriodDto>, IReportOrderCompletedMetricsResponseFactory
{
    public override Task<ReportOrderCompletedMetricsPeriodDto> PrepareDto(ReportOrderCompletedMetricsEntity view)
    {
        var dto = new ReportOrderCompletedMetricsPeriodDto
        {
            Date = view.Date,
            TotalCount = view.TotalCount,
            Period = view.Period
        };

        return Task.FromResult(dto);
    }

    public async Task<ReportOrderCompletedMetricsDto> PrepareMetricsDto(
        ReportOrderCompletedMetricsEntity daily,
        ReportOrderCompletedMetricsEntity monthly,
        ReportOrderCompletedMetricsEntity yearly,
        ReportOrderCompletedMetricsEntity allTime)
    {
        var dto = new ReportOrderCompletedMetricsDto
        {
            ThisDay = await PrepareDto(daily),
            ThisMonth = await PrepareDto(monthly),
            ThisYear = await PrepareDto(yearly),
            AllTime = await PrepareDto(allTime)
        };

        return dto;
    }
}