using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Catalog.Domain.Products
{
    public class ReportProductMetrics : Entity
    {
        public long TotalCount { get; private set; }
        public ReportPeriod Period { get; private set; }

        private ReportProductMetrics() { }

        public ReportProductMetrics(long totalCount, ReportPeriod period)
        {
            TotalCount = totalCount;
            Period = period;
        }

        public void UpdateCount(long totalCount)
        {
            TotalCount = totalCount;
        }
    }
}
