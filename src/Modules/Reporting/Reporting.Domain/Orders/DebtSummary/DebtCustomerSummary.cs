namespace Invoria.Reporting.Domain.Orders.DebtSummary;

public sealed class DebtCustomerSummary : DebtSummaryBase
{
    public DebtCustomerSummary() => SummaryType = DebtSummaryType.Customer;

    public string CustomerId { get; set; } = null!;

    public DateTimeOffset OldestDebtDate { get; set; }

    public decimal OldestDebtAmount { get; set; }
}
