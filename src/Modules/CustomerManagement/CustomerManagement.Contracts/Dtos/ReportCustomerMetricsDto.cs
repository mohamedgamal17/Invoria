namespace Invoria.CustomerManagement.Contracts.Dtos
{
    public class ReportCustomerMetricsDto
    {
        public ReportCustomerMetricsPeriodDto? ThisDay { get; set; }
        public ReportCustomerMetricsPeriodDto? ThisMonth { get; set; }
        public ReportCustomerMetricsPeriodDto? ThisYear { get; set; }
        public ReportCustomerMetricsPeriodDto? AllTime { get; set; }
    }
}
