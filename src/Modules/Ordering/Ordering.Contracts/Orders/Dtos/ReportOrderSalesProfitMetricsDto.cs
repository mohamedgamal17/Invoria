namespace Invoria.Ordering.Contracts.Orders.Dtos;

public class ReportOrderSalesProfitMetricsDto
{
    public ReportOrderSalesProfitMetricsPeriodDto ThisDay { get; set; } = new();
    public ReportOrderSalesProfitMetricsPeriodDto ThisMonth { get; set; } = new();
    public ReportOrderSalesProfitMetricsPeriodDto ThisYear { get; set; } = new();
    public ReportOrderSalesProfitMetricsPeriodDto AllTime { get; set; } = new();
}