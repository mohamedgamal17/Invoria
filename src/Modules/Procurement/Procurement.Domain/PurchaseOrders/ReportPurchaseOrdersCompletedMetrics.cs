using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Procurement.Domain.PurchaseOrders;

public class ReportPurchaseOrdersCompletedMetrics : Entity
{
    public DateTimeOffset Date { get; private set; }
    public long TotalCount { get; private set; }
    public ReportPeriod Period { get; private set; }

    private ReportPurchaseOrdersCompletedMetrics()
    {
    }

    public ReportPurchaseOrdersCompletedMetrics(DateTimeOffset date, long totalCount, ReportPeriod period)
    {
        Date = date;
        TotalCount = totalCount;
        Period = period;
    }

    public ReportPurchaseOrdersCompletedMetrics(string id, DateTimeOffset date, long totalCount, ReportPeriod period)
    {
        Id = id;
        Date = date;
        TotalCount = totalCount;
        Period = period;
    }

    public static string CreateId(ReportPeriod period, DateTimeOffset date)
        => $"{period}-{date:O}";

    public void UpdateCount(long totalCount)
    {
        TotalCount = totalCount;
    }
}