namespace Invoria.Reporting.Application.Orders.Materialization.DebtSummary;

public interface IDebtSummaryRollupRefresher
{
    Task RefreshAsync(CancellationToken cancellationToken);
}
