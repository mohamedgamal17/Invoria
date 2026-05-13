using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;

/// <summary>
/// Full rebuild of <see cref="ReportedOrderStatusByDay"/> from <see cref="ReportedOrder"/>.
/// Uses in-memory grouping of (CreatedAt, OrderStatus) for predictable EF translation across providers.
/// </summary>
public sealed class ReportedOrderStatusSummaryRollupRefresher : IReportedOrderStatusSummaryRollupRefresher
{
    private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";

    private readonly ReportingDbContext _dbContext;
    private readonly ILogger<ReportedOrderStatusSummaryRollupRefresher> _logger;

    public ReportedOrderStatusSummaryRollupRefresher(
        ReportingDbContext dbContext,
        ILogger<ReportedOrderStatusSummaryRollupRefresher> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var isInMemory = string.Equals(
            _dbContext.Database.ProviderName,
            InMemoryProviderName,
            StringComparison.Ordinal);

        if (isInMemory)
        {
            await RefreshCoreAsync(isInMemory, cancellationToken);
            return;
        }

        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await RefreshCoreAsync(isInMemory, cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Order status summary materialization refresh failed.");
            throw;
        }
    }

    private async Task RefreshCoreAsync(bool isInMemory, CancellationToken cancellationToken)
    {
        if (isInMemory)
        {
            var existing = await _dbContext.ReportedOrderStatusByDays.ToListAsync(cancellationToken);
            _dbContext.ReportedOrderStatusByDays.RemoveRange(existing);
        }
        else
        {
            await _dbContext.ReportedOrderStatusByDays.ExecuteDeleteAsync(cancellationToken);
        }

        var sourceRows = await _dbContext.ReportedOrders
            .AsNoTracking()
            .Select(o => new { o.CreatedAt, o.OrderStatus })
            .ToListAsync(cancellationToken);

        var aggregates = sourceRows
            .GroupBy(x => new
            {
                Day = DateOnly.FromDateTime(x.CreatedAt.UtcDateTime),
                x.OrderStatus
            })
            .Select(g => new ReportedOrderStatusByDay
            {
                DayUtc = g.Key.Day,
                OrderStatus = g.Key.OrderStatus,
                Count = g.Count()
            })
            .ToList();

        if (aggregates.Count > 0)
        {
            await _dbContext.ReportedOrderStatusByDays.AddRangeAsync(aggregates, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order status summary materialization refreshed: {AggregateRows} day/status rows from {SourceOrders} orders.",
            aggregates.Count,
            sourceRows.Count);
    }
}
