using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Factories;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

public class ReportPurchaseSalesMetricsResponseFactory : ResponseFactory<ReportPurchaseSalesMetricsEntity, ReportPurchaseSalesMetricsPeriodDto>, IReportPurchaseSalesMetricsResponseFactory
{
    public override Task<ReportPurchaseSalesMetricsPeriodDto> PrepareDto(ReportPurchaseSalesMetricsEntity view)
    {
        var dto = new ReportPurchaseSalesMetricsPeriodDto
        {
            Date = view.Date,
            TotalAmount = view.TotalAmount,
            SubTotal = view.SubTotal,
            TaxAmount = view.TaxAmount,
            DiscountAmount = view.DiscountAmount,
            Period = view.Period
        };

        return Task.FromResult(dto);
    }

    public async Task<ReportPurchaseSalesMetricsDto> PrepareMetricsDto(
        ReportPurchaseSalesMetricsEntity daily,
        ReportPurchaseSalesMetricsEntity monthly,
        ReportPurchaseSalesMetricsEntity yearly,
        ReportPurchaseSalesMetricsEntity allTime)
    {
        var dto = new ReportPurchaseSalesMetricsDto
        {
            ThisDay = await PrepareDto(daily),
            ThisMonth = await PrepareDto(monthly),
            ThisYear = await PrepareDto(yearly),
            AllTime = await PrepareDto(allTime)
        };

        return dto;
    }
}