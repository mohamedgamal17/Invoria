namespace Invoria.Ordering.Contracts.Orders.Dtos;

public class ReportOrderSalesMetricsDto
{
    public ReportOrderSalesMetricsPeriodDto ThisDay { get; set; } = new();
    public ReportOrderSalesMetricsPeriodDto ThisMonth { get; set; } = new();
    public ReportOrderSalesMetricsPeriodDto ThisYear { get; set; } = new();
    public ReportOrderSalesMetricsPeriodDto AllTime { get; set; } = new();
}