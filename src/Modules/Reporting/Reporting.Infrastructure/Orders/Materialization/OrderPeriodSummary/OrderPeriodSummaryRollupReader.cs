using Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.OrderPeriodSummary;

public sealed class OrderPeriodSummaryRollupReader : IOrderPeriodSummaryRollupReader
{
    /// <summary>Stored clock dimension; placed-only rollups always use <c>0</c>.</summary>
    private const int PlacedDateField = 0;

    private readonly ReportingDbContext _dbContext;

    public OrderPeriodSummaryRollupReader(ReportingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<OrderPeriodSummaryDto> Items, int TotalCount)> GetPeriodSummariesPageAsync(
        OrderPeriodSummaryGranularity granularity,
        DateOnly fromDayInclusiveUtc,
        DateOnly toDayInclusiveUtc,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var gran = granularity.ToString();

        var baseQuery = _dbContext.OrderPeriodSummaries
            .AsNoTracking()
            .Where(x => x.Granularity == gran && x.DateField == PlacedDateField)
            .Where(x => x.PeriodStart <= toDayInclusiveUtc && x.PeriodEnd >= fromDayInclusiveUtc)
            .OrderByDescending(x => x.PeriodStart)
            .ThenByDescending(x => x.PeriodKey);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var rows = await baseQuery
            .Skip(skip)
            .Take(take)
            .Select(x => new OrderPeriodSummaryDto
            {
                Granularity = x.Granularity,
                PeriodKey = x.PeriodKey,
                PeriodStart = x.PeriodStart,
                PeriodEnd = x.PeriodEnd,
                OrderCount = x.OrderCount,
                GrossRevenue = x.GrossRevenue,
                NetRevenue = x.NetRevenue,
                DiscountAmount = x.DiscountAmount,
                CancelledCount = x.CancelledCount,
                DeliveredCount = x.DeliveredCount
            })
            .ToListAsync(cancellationToken);

        return (rows, totalCount);
    }
}
