using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.OrderPeriodSummary;

/// <summary>
/// Full rebuild of <see cref="OrderPeriodSummary"/> from <see cref="ReportedOrder"/>.
/// </summary>
public sealed class OrderPeriodSummaryRollupRefresher : IOrderPeriodSummaryRollupRefresher
{
    private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";

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
        }
        else
        {
            await _dbContext.OrderPeriodSummaries.ExecuteDeleteAsync(cancellationToken);
        }

        const int placedClock = 0;

        var orders = await _dbContext.ReportedOrders
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var accumulators = new Dictionary<(string Granularity, string PeriodKey), BucketAccumulator>();

        foreach (var o in orders)
        {
            var eff = o.CreatedAt;
            foreach (var gran in ReportedOrderPeriodBucketing.AllGranularities)
            {
                var b = ReportedOrderPeriodBucketing.GetBucket(eff, gran);
                if (b is null)
                {
                    continue;
                }

                var key = (gran, b.Value.PeriodKey);
                if (!accumulators.TryGetValue(key, out var acc))
                {
                    acc = new BucketAccumulator(b.Value.PeriodStart, b.Value.PeriodEnd);
                    accumulators[key] = acc;
                }

                acc.AddIfNewOrder(o);
            }
        }

        var rows = accumulators
            .Select(kv => new global::Invoria.Reporting.Domain.Orders.OrderPeriodSummary
            {
                Granularity = kv.Key.Granularity,
                PeriodKey = kv.Key.PeriodKey,
                DateField = placedClock,
                PeriodStart = kv.Value.PeriodStart,
                PeriodEnd = kv.Value.PeriodEnd,
                OrderCount = kv.Value.OrderCount,
                GrossRevenue = kv.Value.GrossRevenue,
                NetRevenue = kv.Value.NetRevenue,
                DiscountAmount = 0m,
                CancelledCount = kv.Value.CancelledCount,
                DeliveredCount = kv.Value.DeliveredCount
            })
            .ToList();

        if (rows.Count > 0)
        {
            await _dbContext.OrderPeriodSummaries.AddRangeAsync(rows, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order period summary materialization refreshed: {RowCount} rows from {SourceOrders} orders.",
            rows.Count,
            orders.Count);
    }

    private sealed class BucketAccumulator
    {
        private readonly HashSet<string> _orderIds = new(StringComparer.Ordinal);

        public BucketAccumulator(DateOnly periodStart, DateOnly periodEnd)
        {
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
        }

        public DateOnly PeriodStart { get; }
        public DateOnly PeriodEnd { get; }

        public int OrderCount => _orderIds.Count;

        public decimal GrossRevenue { get; private set; }

        public decimal NetRevenue { get; private set; }

        public int CancelledCount { get; private set; }

        public int DeliveredCount { get; private set; }

        public void AddIfNewOrder(ReportedOrder o)
        {
            if (!_orderIds.Add(o.Id))
            {
                return;
            }

            GrossRevenue += o.TotalOrderAmount;
            NetRevenue += o.AmountPaid;
            if (o.OrderStatus == OrderStatus.Cancelled)
            {
                CancelledCount++;
            }

            if (o.OrderStatus == OrderStatus.Completed)
            {
                DeliveredCount++;
            }
        }
    }
}
