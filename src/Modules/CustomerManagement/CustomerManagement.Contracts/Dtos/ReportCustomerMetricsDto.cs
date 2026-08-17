namespace Invoria.CustomerManagement.Contracts.Dtos
{
    public class ReportCustomerMetricsDto
    {
        public ReportCustomerMetricsPeriodDto ThisDay { get; set; } = new();
        public ReportCustomerMetricsPeriodDto ThisMonth { get; set; } = new();
        public ReportCustomerMetricsPeriodDto ThisYear { get; set; } = new();
        public ReportCustomerMetricsPeriodDto AllTime { get; set; } = new();
    }
}
