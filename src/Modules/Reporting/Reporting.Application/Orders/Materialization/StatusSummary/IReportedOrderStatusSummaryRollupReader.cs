using Invoria.Reporting.Contracts.Dtos;

namespace Invoria.Reporting.Application.Orders.Materialization.StatusSummary;

/// <summary>
/// Reads the materialized order-status-by-day summary and aggregates counts by status.
/// When <paramref name="fromDayInclusiveUtc"/> and <paramref name="toDayInclusiveUtc"/> are both set,
/// only rows on those UTC calendar days (inclusive) are included; when both are null, all materialized rows are included.
/// </summary>
public interface IReportedOrderStatusSummaryRollupReader
{
    Task<IReadOnlyList<OrderStatusSummaryItemDto>> GetAggregatedByStatusAsync(
        DateOnly? fromDayInclusiveUtc,
        DateOnly? toDayInclusiveUtc,
        CancellationToken cancellationToken);
}
