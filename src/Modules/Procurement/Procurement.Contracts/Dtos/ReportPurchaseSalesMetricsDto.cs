namespace Invoria.Procurement.Contracts.Dtos;

public class ReportPurchaseSalesMetricsDto
{
    public ReportPurchaseSalesMetricsPeriodDto ThisDay { get; set; } = new();
    public ReportPurchaseSalesMetricsPeriodDto ThisMonth { get; set; } = new();
    public ReportPurchaseSalesMetricsPeriodDto ThisYear { get; set; } = new();
    public ReportPurchaseSalesMetricsPeriodDto AllTime { get; set; } = new();
}