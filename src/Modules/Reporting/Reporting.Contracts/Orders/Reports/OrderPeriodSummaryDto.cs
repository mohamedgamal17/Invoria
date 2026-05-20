namespace Invoria.Reporting.Contracts.Orders.Reports;

public sealed class OrderPeriodSummaryDto
{
    public string Granularity { get; set; } = null!;

    public string PeriodKey { get; set; } = null!;

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public int OrderCount { get; set; }

    public decimal GrossRevenue { get; set; }

    public decimal NetRevenue { get; set; }

    public decimal DiscountAmount { get; set; }

    public int CancelledCount { get; set; }

    public int DeliveredCount { get; set; }
}
