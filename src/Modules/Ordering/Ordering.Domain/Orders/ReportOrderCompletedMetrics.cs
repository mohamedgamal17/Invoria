using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Domain.Orders;

public class ReportOrderCompletedMetrics : Entity
{
    public DateTimeOffset Date { get; private set; }
    public long TotalCount { get; private set; }
    public ReportPeriod Period { get; private set; }

    private ReportOrderCompletedMetrics() { }

    public ReportOrderCompletedMetrics(DateTimeOffset date, long totalCount, ReportPeriod period)
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
