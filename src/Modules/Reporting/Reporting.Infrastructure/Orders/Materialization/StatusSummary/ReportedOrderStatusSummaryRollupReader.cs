using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Invoria.Reporting.Contracts.Dtos;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;

public sealed class ReportedOrderStatusSummaryRollupReader : IReportedOrderStatusSummaryRollupReader
{
    private readonly ReportingDbContext _dbContext;

    public ReportedOrderStatusSummaryRollupReader(ReportingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OrderStatusSummaryItemDto>> GetAggregatedByStatusAsync(
        DateOnly? fromDayInclusiveUtc,
        DateOnly? toDayInclusiveUtc,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.ReportedOrderStatusByDays.AsNoTracking();

        if (fromDayInclusiveUtc is not null && toDayInclusiveUtc is not null)
        {
            query = query.Where(r => r.DayUtc >= fromDayInclusiveUtc && r.DayUtc <= toDayInclusiveUtc);
        }

        return await query
            .GroupBy(r => r.OrderStatus)
            .Select(g => new OrderStatusSummaryItemDto
            {
                Status = g.Key,
                Count = g.Sum(x => x.Count)
            })
            .OrderBy(x => x.Status)
            .ToListAsync(cancellationToken);
    }
}
