using Invoria.Reporting.Application.Orders.Materialization.DebtSummary;
using Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;

/// <summary>
/// Periodically rebuilds reporting materialized rollups (order status by day, order period summaries, and debt summaries; default 5 minutes).
/// </summary>
public sealed class ReportedOrderStatusSummaryRollupRefreshHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportedOrderStatusSummaryRollupRefreshHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public ReportedOrderStatusSummaryRollupRefreshHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReportedOrderStatusSummaryRollupRefreshHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunRefreshAsync(stoppingToken);
            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RunRefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var statusRefresher = scope.ServiceProvider.GetRequiredService<IReportedOrderStatusSummaryRollupRefresher>();
            await statusRefresher.RefreshAsync(cancellationToken);

            var periodRefresher = scope.ServiceProvider.GetRequiredService<IOrderPeriodSummaryRollupRefresher>();
            await periodRefresher.RefreshAsync(cancellationToken);

            var debtRefresher = scope.ServiceProvider.GetRequiredService<IDebtSummaryRollupRefresher>();
            await debtRefresher.RefreshAsync(cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Scheduled order status summary materialization refresh failed; will retry on next interval.");
        }
    }
}
