using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Factories;

using ReportPurchaseOrdersCompletedMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseOrdersCompletedMetrics;

public class ReportPurchaseOrdersCompletedMetricsResponseFactory : ResponseFactory<ReportPurchaseOrdersCompletedMetricsEntity, ReportPurchaseOrdersCompletedMetricsPeriodDto>, IReportPurchaseOrdersCompletedMetricsResponseFactory
{
    public override Task<ReportPurchaseOrdersCompletedMetricsPeriodDto> PrepareDto(ReportPurchaseOrdersCompletedMetricsEntity view)
    {
        var dto = new ReportPurchaseOrdersCompletedMetricsPeriodDto
        {
            Date = view.Date,
            TotalCount = view.TotalCount,
            Period = view.Period
        };

        return Task.FromResult(dto);
    }

    public async Task<ReportPurchaseOrdersCompletedMetricsDto> PrepareMetricsDto(
        ReportPurchaseOrdersCompletedMetricsEntity daily,
        ReportPurchaseOrdersCompletedMetricsEntity monthly,
        ReportPurchaseOrdersCompletedMetricsEntity yearly,
        ReportPurchaseOrdersCompletedMetricsEntity allTime)
    {
        var dto = new ReportPurchaseOrdersCompletedMetricsDto
        {
            ThisDay = await PrepareDto(daily),
            ThisMonth = await PrepareDto(monthly),
            ThisYear = await PrepareDto(yearly),
            AllTime = await PrepareDto(allTime)
        };

        return dto;
    }
}