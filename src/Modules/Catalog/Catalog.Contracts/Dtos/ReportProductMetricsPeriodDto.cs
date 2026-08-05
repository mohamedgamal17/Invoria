using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Catalog.Contracts.Dtos
{
    public class ReportProductMetricsPeriodDto
    {
        public DateTimeOffset Date { get; set; }
        public long TotalCount { get; set; }
        public ReportPeriod Period { get; set; }
    }
}
