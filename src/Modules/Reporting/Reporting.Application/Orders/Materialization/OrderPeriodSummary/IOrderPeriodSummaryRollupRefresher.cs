namespace Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;

public interface IOrderPeriodSummaryRollupRefresher
{
    Task RefreshAsync(CancellationToken cancellationToken);
}
