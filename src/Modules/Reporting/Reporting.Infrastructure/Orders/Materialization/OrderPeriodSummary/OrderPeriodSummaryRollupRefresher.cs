using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Infrastructure.EntityFramework;
using OrderPeriodSummaryEntity = Invoria.Reporting.Domain.Orders.OrderPeriodSummary.OrderPeriodSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.OrderPeriodSummary;

/// <summary>
/// Full rebuild of <see cref="Invoria.Reporting.Domain.Orders.OrderPeriodSummary.OrderPeriodSummary"/> from <see cref="Invoria.Reporting.Domain.Orders.ReportedOrder"/>.
/// </summary>
public sealed class OrderPeriodSummaryRollupRefresher : IOrderPeriodSummaryRollupRefresher
{
    private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";
    private const int MaxChunkSize = 50;

    private readonly ReportingDbContext _dbContext;
    private readonly ILogger<OrderPeriodSummaryRollupRefresher> _logger;

    public OrderPeriodSummaryRollupRefresher(
        ReportingDbContext dbContext,
        ILogger<OrderPeriodSummaryRollupRefresher> logger)
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

        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await RefreshCoreAsync(isInMemory, cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Order period summary materialization refresh failed.");
            throw;
        }
    }

    private async Task RefreshCoreAsync(bool isInMemory, CancellationToken cancellationToken)
    {
        if (isInMemory)
        {
            var existing = await _dbContext.OrderPeriodSummaries.ToListAsync(cancellationToken);
            _dbContext.OrderPeriodSummaries.RemoveRange(existing);
            if (existing.Count > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            await _dbContext.OrderPeriodSummaries.ExecuteDeleteAsync(cancellationToken);
        }

        const int placedClock = 0;
        var sourceOrders = _dbContext.ReportedOrders.AsQueryable();
        var totalSourceOrders = await sourceOrders.CountAsync(cancellationToken);

        foreach (var granularity in ReportedOrderPeriodBucketing.AllGranularities)
        {
            var skip = 0;

            while (true)
            {
                var (aggregates, sourceOrderCount) = await FetchChunkAggregatesAsync(
                    sourceOrders,
                    granularity,
                    skip,
                    MaxChunkSize,
                    cancellationToken);

                if (sourceOrderCount == 0)
                {
                    break;
                }

                await UpsertChunkAsync(granularity, placedClock, aggregates, cancellationToken);
                skip += sourceOrderCount;
            }
        }

        var totalRows = await _dbContext.OrderPeriodSummaries.CountAsync(cancellationToken);

        _logger.LogInformation(
            "Order period summary materialization refreshed: {RowCount} rows from {SourceOrders} orders.",
            totalRows,
            totalSourceOrders);
    }

    private async Task UpsertChunkAsync(
        string granularity,
        int placedClock,
        IReadOnlyList<PeriodChunkAggregate> aggregates,
        CancellationToken cancellationToken)
    {
        if (aggregates.Count == 0)
        {
            return;
        }

        var periodKeys = aggregates.Select(a => a.PeriodKey).ToList();

        var existing = await _dbContext.OrderPeriodSummaries
            .Where(x => x.Granularity == granularity
                        && x.DateField == placedClock
                        && periodKeys.Contains(x.PeriodKey))
            .ToDictionaryAsync(x => x.PeriodKey, StringComparer.Ordinal, cancellationToken);

        foreach (var partial in aggregates)
        {
            if (existing.TryGetValue(partial.PeriodKey, out var row))
            {
                row.OrderCount += partial.OrderCount;
                row.GrossRevenue += partial.GrossRevenue;
                row.NetRevenue += partial.NetRevenue;
                row.CancelledCount += partial.CancelledCount;
                row.DeliveredCount += partial.DeliveredCount;
                continue;
            }

            var inserted = new OrderPeriodSummaryEntity
            {
                Granularity = granularity,
                PeriodKey = partial.PeriodKey,
                DateField = placedClock,
                PeriodStart = partial.PeriodStart,
                PeriodEnd = partial.PeriodEnd,
                OrderCount = partial.OrderCount,
                GrossRevenue = partial.GrossRevenue,
                NetRevenue = partial.NetRevenue,
                DiscountAmount = 0m,
                CancelledCount = partial.CancelledCount,
                DeliveredCount = partial.DeliveredCount
            };

            _dbContext.OrderPeriodSummaries.Add(inserted);
            existing[partial.PeriodKey] = inserted;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<(IReadOnlyList<PeriodChunkAggregate> Aggregates, int SourceOrderCount)> FetchChunkAggregatesAsync(
        IQueryable<ReportedOrder> orders,
        string granularity,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var chunk = orders.AsNoTracking().OrderBy(o => o.Id).Skip(skip).Take(take);
        return await FetchChunkAggregatesForChunkAsync(chunk, granularity, cancellationToken);
    }

    private static async Task<(IReadOnlyList<PeriodChunkAggregate>, int)> FetchChunkAggregatesForChunkAsync(
        IQueryable<ReportedOrder> chunk,
        string granularity,
        CancellationToken cancellationToken)
    {
        var slices = await chunk
            .Select(o => new OrderSlice(
                o.CreatedAt,
                o.TotalOrderAmount,
                o.AmountPaid,
                o.OrderStatus))
            .ToListAsync(cancellationToken);

        if (slices.Count == 0)
        {
            return ([], 0);
        }

        var aggregates = slices
            .Select(s =>
            {
                var bucket = ReportedOrderPeriodBucketing.GetBucket(s.CreatedAt, granularity);
                return (Slice: s, Bucket: bucket);
            })
            .Where(x => x.Bucket is not null)
            .GroupBy(x => x.Bucket!.Value.PeriodKey)
            .Select(g =>
            {
                var first = g.First().Bucket!.Value;
                return new PeriodChunkAggregate(
                    g.Key,
                    first.PeriodStart,
                    first.PeriodEnd,
                    g.Count(),
                    g.Sum(x => x.Slice.TotalOrderAmount),
                    g.Sum(x => x.Slice.AmountPaid),
                    g.Count(x => x.Slice.OrderStatus == OrderStatus.Cancelled),
                    g.Count(x => x.Slice.OrderStatus == OrderStatus.Completed));
            })
            .ToList();

        return (aggregates, slices.Count);
    }

    private sealed record OrderSlice(
        DateTimeOffset CreatedAt,
        decimal TotalOrderAmount,
        decimal AmountPaid,
        OrderStatus OrderStatus);
}
