using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Factories;

using ReportPurchaseOrdersCompletedMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseOrdersCompletedMetrics;

public interface IReportPurchaseOrdersCompletedMetricsResponseFactory : IResponseFactory<ReportPurchaseOrdersCompletedMetricsEntity, ReportPurchaseOrdersCompletedMetricsPeriodDto>
{
    Task<ReportPurchaseOrdersCompletedMetricsDto> PrepareMetricsDto(
        ReportPurchaseOrdersCompletedMetricsEntity daily,
        ReportPurchaseOrdersCompletedMetricsEntity monthly,
        ReportPurchaseOrdersCompletedMetricsEntity yearly,
        ReportPurchaseOrdersCompletedMetricsEntity allTime);
}