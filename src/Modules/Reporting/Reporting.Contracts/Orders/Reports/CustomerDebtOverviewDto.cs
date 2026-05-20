namespace Invoria.Reporting.Contracts.Orders.Reports;

public sealed class CustomerDebtOverviewDto
{
    public string CustomerId { get; set; } = null!;

    public decimal TotalOutstanding { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal TotalOrderValue { get; set; }

    public int DebtOrderCount { get; set; }

    public int PartiallyPaidCount { get; set; }

    public int UnpaidCount { get; set; }

    public DateTimeOffset OldestDebtDate { get; set; }

    public decimal OldestDebtAmount { get; set; }

    public DateTimeOffset ComputedAt { get; set; }
}
