namespace Invoria.Ordering.Contracts.Orders.Dtos;

public class ReportOrderCompletedMetricsDto
{
    public ReportOrderCompletedMetricsPeriodDto ThisDay { get; set; } = new();
    public ReportOrderCompletedMetricsPeriodDto ThisMonth { get; set; } = new();
    public ReportOrderCompletedMetricsPeriodDto ThisYear { get; set; } = new();
    public ReportOrderCompletedMetricsPeriodDto AllTime { get; set; } = new();
}