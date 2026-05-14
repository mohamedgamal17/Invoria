namespace Invoria.Reporting.Domain.Orders;

/// <summary>
/// Materialized aggregate: order counts and revenue per calendar period bucket, rebuilt periodically from <see cref="ReportedOrder"/>.
/// Granularity discriminates Day vs Week vs Month. <see cref="OrderPeriodSummary.DateField"/> is reserved for future clock dimensions; placed-only rollups always store <c>0</c> there.
/// </summary>
public sealed class OrderPeriodSummary
{
    public string Granularity { get; set; } = null!;

    public string PeriodKey { get; set; } = null!;

    /// <summary>Clock dimension as int; placed-only materialization uses <c>0</c>.</summary>
    public int DateField { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public int OrderCount { get; set; }

    public decimal GrossRevenue { get; set; }

    public decimal NetRevenue { get; set; }

    public decimal DiscountAmount { get; set; }

    public int CancelledCount { get; set; }

    public int DeliveredCount { get; set; }
}
