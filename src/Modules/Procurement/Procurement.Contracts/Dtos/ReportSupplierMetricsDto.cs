namespace Invoria.Procurement.Contracts.Dtos;

public class ReportSupplierMetricsDto
{
    public ReportSupplierMetricsPeriodDto ThisDay { get; set; } = new();
    public ReportSupplierMetricsPeriodDto ThisMonth { get; set; } = new();
    public ReportSupplierMetricsPeriodDto ThisYear { get; set; } = new();
    public ReportSupplierMetricsPeriodDto AllTime { get; set; } = new();
}