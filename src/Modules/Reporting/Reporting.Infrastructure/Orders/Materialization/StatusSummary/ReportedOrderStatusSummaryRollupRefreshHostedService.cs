using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;

/// <summary>
/// Periodically rebuilds the order status by day materialized summary (default 5 minutes).
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
            var refresher = scope.ServiceProvider.GetRequiredService<IReportedOrderStatusSummaryRollupRefresher>();
            await refresher.RefreshAsync(cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Scheduled order status summary materialization refresh failed; will retry on next interval.");
        }
    }
}
