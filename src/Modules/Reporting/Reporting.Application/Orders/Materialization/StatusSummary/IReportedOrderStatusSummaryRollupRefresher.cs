namespace Invoria.Reporting.Application.Orders.Materialization.StatusSummary;

/// <summary>
/// Rebuilds the materialized order-status-by-day summary from <c>ReportedOrders</c> (full refresh).
/// Intended to run on a fixed interval; results may lag live data by one interval.
/// </summary>
public interface IReportedOrderStatusSummaryRollupRefresher
{
    Task RefreshAsync(CancellationToken cancellationToken);
}
