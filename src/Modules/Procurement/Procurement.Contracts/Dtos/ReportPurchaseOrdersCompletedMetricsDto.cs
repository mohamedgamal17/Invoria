namespace Invoria.Procurement.Contracts.Dtos;

public class ReportPurchaseOrdersCompletedMetricsDto
{
    public ReportPurchaseOrdersCompletedMetricsPeriodDto ThisDay { get; set; } = new();
    public ReportPurchaseOrdersCompletedMetricsPeriodDto ThisMonth { get; set; } = new();
    public ReportPurchaseOrdersCompletedMetricsPeriodDto ThisYear { get; set; } = new();
    public ReportPurchaseOrdersCompletedMetricsPeriodDto AllTime { get; set; } = new();
}