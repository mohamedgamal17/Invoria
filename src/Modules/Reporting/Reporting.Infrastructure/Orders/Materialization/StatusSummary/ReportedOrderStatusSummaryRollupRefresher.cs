using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.StatusSummary;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;

/// <summary>
/// Full rebuild of <see cref="ReportedOrderStatusByDay"/> from <see cref="ReportedOrder"/>.
/// </summary>
public sealed class ReportedOrderStatusSummaryRollupRefresher : IReportedOrderStatusSummaryRollupRefresher
{
    private const int MaxChunkSize = 50;

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
        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await RefreshCoreAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Order status summary materialization refresh failed.");
            throw;
        }
    }

    private async Task RefreshCoreAsync(CancellationToken cancellationToken)
    {
        await _dbContext.ReportedOrderStatusByDays.ExecuteDeleteAsync(cancellationToken);

        var sourceOrders = _dbContext.ReportedOrders.AsQueryable();
        var totalSourceOrders = await sourceOrders.CountAsync(cancellationToken);
        var skip = 0;

        while (true)
        {
            var (aggregates, sourceOrderCount) = await FetchChunkAggregatesAsync(
                sourceOrders,
                skip,
                MaxChunkSize,
                cancellationToken);

            if (sourceOrderCount == 0)
            {
                break;
            }

            await UpsertChunkAsync(aggregates, cancellationToken);
            skip += sourceOrderCount;
        }

        var totalRows = await _dbContext.ReportedOrderStatusByDays.CountAsync(cancellationToken);

        _logger.LogInformation(
            "Order status summary materialization refreshed: {AggregateRows} day/status rows from {SourceOrders} orders.",
            totalRows,
            totalSourceOrders);
    }

    private async Task UpsertChunkAsync(
        IReadOnlyList<StatusChunkAggregate> aggregates,
        CancellationToken cancellationToken)
    {
        if (aggregates.Count == 0)
        {
            return;
        }

        var days = aggregates.Select(a => a.DayUtc).Distinct().ToList();
        var statuses = aggregates.Select(a => a.OrderStatus).Distinct().ToList();

        var existingRows = await _dbContext.ReportedOrderStatusByDays
            .Where(x => days.Contains(x.DayUtc) && statuses.Contains(x.OrderStatus))
            .ToListAsync(cancellationToken);

        var existing = existingRows.ToDictionary(x => (x.DayUtc, x.OrderStatus));

        foreach (var partial in aggregates)
        {
            var key = (partial.DayUtc, partial.OrderStatus);
            if (existing.TryGetValue(key, out var row))
            {
                row.Count += partial.Count;
                continue;
            }

            var inserted = new ReportedOrderStatusByDay
            {
                DayUtc = partial.DayUtc,
                OrderStatus = partial.OrderStatus,
                Count = partial.Count
            };

            _dbContext.ReportedOrderStatusByDays.Add(inserted);
            existing[key] = inserted;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<(IReadOnlyList<StatusChunkAggregate> Aggregates, int SourceOrderCount)> FetchChunkAggregatesAsync(
        IQueryable<ReportedOrder> orders,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var slices = await orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .Skip(skip)
            .Take(take)
            .Select(o => new OrderSlice(o.CreatedAt, o.OrderStatus))
            .ToListAsync(cancellationToken);

        if (slices.Count == 0)
        {
            return ([], 0);
        }

        var aggregates = slices
            .GroupBy(x => new
            {
                Day = DateOnly.FromDateTime(x.CreatedAt.UtcDateTime),
                x.OrderStatus
            })
            .Select(g => new StatusChunkAggregate(
                g.Key.Day,
                g.Key.OrderStatus,
                g.Count()))
            .ToList();

        return (aggregates, slices.Count);
    }

    private sealed record OrderSlice(DateTimeOffset CreatedAt, OrderStatus OrderStatus);
}
