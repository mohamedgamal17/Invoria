namespace Invoria.Catalog.Contracts.Dtos
{
    public class ReportProductMetricsDto
    {
        public ReportProductMetricsPeriodDto ThisDay { get; set; } = new();
        public ReportProductMetricsPeriodDto ThisMonth { get; set; } = new();
        public ReportProductMetricsPeriodDto ThisYear { get; set; } = new();
        public ReportProductMetricsPeriodDto AllTime { get; set; } = new();
    }
}
