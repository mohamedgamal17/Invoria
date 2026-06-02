using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Reporting.Application.Orders.Materialization.DebtSummary;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.DebtSummary;
using Invoria.Reporting.Infrastructure.EntityFramework;
using DebtCustomerSummaryEntity = Invoria.Reporting.Domain.Orders.DebtSummary.DebtCustomerSummary;
using DebtGlobalSummaryEntity = Invoria.Reporting.Domain.Orders.DebtSummary.DebtGlobalSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.DebtSummary;

/// <summary>
/// Full rebuild of <see cref="DebtSummaryBase"/> from completed <see cref="ReportedOrder"/> rows with outstanding balance.
/// </summary>
public sealed class DebtSummaryRollupRefresher : IDebtSummaryRollupRefresher
{
    private const int MaxChunkSize = 50;

    private readonly ReportingDbContext _dbContext;
    private readonly ILogger<DebtSummaryRollupRefresher> _logger;

    public DebtSummaryRollupRefresher(
        ReportingDbContext dbContext,
        ILogger<DebtSummaryRollupRefresher> logger)
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
            _logger.LogError(ex, "Debt summary materialization refresh failed.");
            throw;
        }
    }

    private async Task RefreshCoreAsync(CancellationToken cancellationToken)
    {
        _dbContext.ChangeTracker.Clear();
        await _dbContext.DebtSummaries.ExecuteDeleteAsync(cancellationToken);

        var computedAt = DateTimeOffset.UtcNow;
        var sourceOrders = EligibleDebtOrders(_dbContext.ReportedOrders);
        var totalEligibleOrders = await sourceOrders.CountAsync(cancellationToken);

        await RefreshGlobalSummariesAsync(sourceOrders, computedAt, cancellationToken);
        await RefreshCustomerSummariesAsync(sourceOrders, computedAt, cancellationToken);

        var globalCount = await _dbContext.DebtSummaries.OfType<DebtGlobalSummaryEntity>().CountAsync(cancellationToken);
        var customerCount = await _dbContext.DebtSummaries.OfType<DebtCustomerSummaryEntity>().CountAsync(cancellationToken);

        _logger.LogInformation(
            "Debt summary materialization refreshed: {GlobalRows} global and {CustomerRows} customer rows from {EligibleOrders} completed orders with outstanding balance.",
            globalCount,
            customerCount,
            totalEligibleOrders);
    }

    private static IQueryable<ReportedOrder> EligibleDebtOrders(IQueryable<ReportedOrder> orders) =>
        orders.Where(o =>
            o.OrderStatus == OrderStatus.Completed
            && o.AmountOutstanding > 0);

    private async Task RefreshGlobalSummariesAsync(
        IQueryable<ReportedOrder> sourceOrders,
        DateTimeOffset computedAt,
        CancellationToken cancellationToken)
    {
        var metrics = await sourceOrders
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalOutstanding = g.Sum(x => x.AmountOutstanding),
                TotalPaid = g.Sum(x => x.AmountPaid),
                TotalOrderValue = g.Sum(x => x.TotalOrderAmount),
                DebtOrderCount = g.Count(),
                PartiallyPaidCount = g.Count(x => x.PaymentStatus == OrderPaymentStatus.Partial),
                UnpaidCount = g.Count(x => x.PaymentStatus == OrderPaymentStatus.Unpaid),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var global = new DebtGlobalSummaryEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            ComputedAt = computedAt,
            TotalOutstanding = metrics?.TotalOutstanding ?? 0m,
            TotalPaid = metrics?.TotalPaid ?? 0m,
            TotalOrderValue = metrics?.TotalOrderValue ?? 0m,
            DebtOrderCount = metrics?.DebtOrderCount ?? 0,
            PartiallyPaidCount = metrics?.PartiallyPaidCount ?? 0,
            UnpaidCount = metrics?.UnpaidCount ?? 0,
        };

        _dbContext.DebtSummaries.Add(global);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RefreshCustomerSummariesAsync(
        IQueryable<ReportedOrder> sourceOrders,
        DateTimeOffset computedAt,
        CancellationToken cancellationToken)
    {
        var skip = 0;

        while (true)
        {
            var (customerAggregates, sourceOrderCount) = await FetchCustomerChunkAggregatesAsync(
                sourceOrders,
                skip,
                MaxChunkSize,
                cancellationToken);

            if (sourceOrderCount == 0)
            {
                break;
            }

            await UpsertCustomerChunkAsync(customerAggregates, computedAt, cancellationToken);
            skip += sourceOrderCount;
        }
    }

    private async Task UpsertCustomerChunkAsync(
        IReadOnlyList<DebtCustomerChunkAggregate> customerAggregates,
        DateTimeOffset computedAt,
        CancellationToken cancellationToken)
    {
        if (customerAggregates.Count == 0)
        {
            return;
        }

        var customerIds = customerAggregates.Select(a => a.CustomerId).ToList();

        var existingCustomers = await _dbContext.DebtSummaries
            .OfType<DebtCustomerSummaryEntity>()
            .Where(x => customerIds.Contains(x.CustomerId))
            .ToDictionaryAsync(x => x.CustomerId, StringComparer.Ordinal, cancellationToken);

        foreach (var partial in customerAggregates)
        {
            if (existingCustomers.TryGetValue(partial.CustomerId, out var row))
            {
                MergeCustomerMetrics(row, partial);
                row.ComputedAt = computedAt;
                continue;
            }

            var inserted = new DebtCustomerSummaryEntity
            {
                Id = partial.CustomerId,
                CustomerId = partial.CustomerId,
                TotalOutstanding = partial.TotalOutstanding,
                TotalPaid = partial.TotalPaid,
                TotalOrderValue = partial.TotalOrderValue,
                DebtOrderCount = partial.DebtOrderCount,
                PartiallyPaidCount = partial.PartiallyPaidCount,
                UnpaidCount = partial.UnpaidCount,
                OldestDebtDate = partial.OldestDebtDate ?? default,
                OldestDebtAmount = partial.OldestDebtAmount,
                ComputedAt = computedAt
            };

            _dbContext.DebtSummaries.Add(inserted);
            existingCustomers[partial.CustomerId] = inserted;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void MergeCustomerMetrics(DebtCustomerSummaryEntity row, DebtCustomerChunkAggregate partial)
    {
        row.TotalOutstanding += partial.TotalOutstanding;
        row.TotalPaid += partial.TotalPaid;
        row.TotalOrderValue += partial.TotalOrderValue;
        row.DebtOrderCount += partial.DebtOrderCount;
        row.PartiallyPaidCount += partial.PartiallyPaidCount;
        row.UnpaidCount += partial.UnpaidCount;

        var merged = MergeOldestDebt(
            row.OldestDebtAmount > 0 ? row.OldestDebtDate : null,
            row.OldestDebtAmount,
            partial.OldestDebtDate,
            partial.OldestDebtAmount);
        row.OldestDebtDate = merged.Date ?? default;
        row.OldestDebtAmount = merged.Amount;
    }

    private static async Task<(IReadOnlyList<DebtCustomerChunkAggregate> Aggregates, int SourceOrderCount)> FetchCustomerChunkAggregatesAsync(
        IQueryable<ReportedOrder> orders,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var slices = await FetchOrderSlicesAsync(orders, skip, take, cancellationToken);
        if (slices.Count == 0)
        {
            return ([], 0);
        }

        var aggregates = slices
            .GroupBy(x => x.CustomerId)
            .Select(g => BuildCustomerAggregate(g.Key, g))
            .ToList();

        return (aggregates, slices.Count);
    }

    private static async Task<List<DebtOrderSlice>> FetchOrderSlicesAsync(
        IQueryable<ReportedOrder> orders,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .Skip(skip)
            .Take(take)
            .Select(o => new DebtOrderSlice(
                o.CustomerId,
                o.TotalOrderAmount,
                o.AmountPaid,
                o.AmountOutstanding,
                o.PaymentStatus,
                o.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    private static DebtCustomerChunkAggregate BuildCustomerAggregate(
        string customerId,
        IEnumerable<DebtOrderSlice> slices)
    {
        var list = slices.ToList();
        var oldest = SelectOldestDebt(list);

        return new DebtCustomerChunkAggregate(
            customerId,
            list.Sum(x => x.AmountOutstanding),
            list.Sum(x => x.AmountPaid),
            list.Sum(x => x.TotalOrderAmount),
            list.Count,
            list.Count(x => x.PaymentStatus == OrderPaymentStatus.Partial),
            list.Count(x => x.PaymentStatus == OrderPaymentStatus.Unpaid),
            oldest.Date,
            oldest.Amount);
    }

    private static (DateTimeOffset? Date, decimal Amount) SelectOldestDebt(IEnumerable<DebtOrderSlice> slices)
    {
        DateTimeOffset? oldestDate = null;
        var oldestAmount = 0m;

        foreach (var slice in slices)
        {
            var merged = MergeOldestDebt(oldestDate, oldestAmount, slice.CreatedAt, slice.AmountOutstanding);
            oldestDate = merged.Date;
            oldestAmount = merged.Amount;
        }

        return (oldestDate, oldestAmount);
    }

    private static (DateTimeOffset? Date, decimal Amount) MergeOldestDebt(
        DateTimeOffset? currentDate,
        decimal currentAmount,
        DateTimeOffset? candidateDate,
        decimal candidateAmount)
    {
        if (candidateAmount <= 0)
        {
            return (currentDate, currentAmount);
        }

        if (currentDate is null || currentAmount <= 0)
        {
            return (candidateDate, candidateAmount);
        }

        if (candidateDate!.Value < currentDate.Value)
        {
            return (candidateDate, candidateAmount);
        }

        if (candidateDate.Value == currentDate.Value && candidateAmount > currentAmount)
        {
            return (candidateDate, candidateAmount);
        }

        return (currentDate, currentAmount);
    }

    private sealed record DebtOrderSlice(
        string CustomerId,
        decimal TotalOrderAmount,
        decimal AmountPaid,
        decimal AmountOutstanding,
        OrderPaymentStatus PaymentStatus,
        DateTimeOffset CreatedAt);
}
