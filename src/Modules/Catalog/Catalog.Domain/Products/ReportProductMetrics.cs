using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Catalog.Domain.Products
{
    public class ReportProductMetrics : Entity
    {
        public DateTimeOffset Date { get; private set; }
        public long TotalCount { get; private set; }
        public ReportPeriod Period { get; private set; }

        private ReportProductMetrics() { }

        public ReportProductMetrics(DateTimeOffset date, long totalCount, ReportPeriod period)
        {
            Date = date;
            TotalCount = totalCount;
            Period = period;
        }

        public void UpdateCount(long totalCount)
        {
            TotalCount = totalCount;
        }
    }
}
