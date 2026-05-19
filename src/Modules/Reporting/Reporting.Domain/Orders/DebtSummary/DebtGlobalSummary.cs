namespace Invoria.Reporting.Domain.Orders.DebtSummary;

public sealed class DebtGlobalSummary : DebtSummaryBase
{
    public DebtGlobalSummary() => SummaryType = DebtSummaryType.Global;

    public decimal CollectionRate =>
        TotalOrderValue == 0 ? 0 : Math.Round((TotalPaid / TotalOrderValue) * 100, 2);
}
