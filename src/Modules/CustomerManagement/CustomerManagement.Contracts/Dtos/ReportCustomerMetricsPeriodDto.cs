using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.CustomerManagement.Contracts.Dtos
{
    public class ReportCustomerMetricsPeriodDto
    {
        public DateTimeOffset Date { get; set; }
        public long TotalCount { get; set; }
        public ReportPeriod Period { get; set; }
    }
}
