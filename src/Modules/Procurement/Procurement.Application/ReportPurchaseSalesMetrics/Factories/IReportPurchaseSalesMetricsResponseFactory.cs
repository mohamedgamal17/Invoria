using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Factories;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

public interface IReportPurchaseSalesMetricsResponseFactory : IResponseFactory<ReportPurchaseSalesMetricsEntity, ReportPurchaseSalesMetricsPeriodDto>
{
    Task<ReportPurchaseSalesMetricsDto> PrepareMetricsDto(
        ReportPurchaseSalesMetricsEntity daily,
        ReportPurchaseSalesMetricsEntity monthly,
        ReportPurchaseSalesMetricsEntity yearly,
        ReportPurchaseSalesMetricsEntity allTime);
}