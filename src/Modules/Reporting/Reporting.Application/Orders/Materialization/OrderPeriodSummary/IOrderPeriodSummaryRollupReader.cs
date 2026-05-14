using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;

public interface IOrderPeriodSummaryRollupReader
{
    /// <summary>Reads the placed-date (UTC) rollup slice only (<c>DateField == 0</c> in storage).</summary>
    Task<(IReadOnlyList<OrderPeriodSummaryDto> Items, int TotalCount)> GetPeriodSummariesPageAsync(
        OrderPeriodSummaryGranularity granularity,
        DateOnly fromDayInclusiveUtc,
        DateOnly toDayInclusiveUtc,
        int skip,
        int take,
        CancellationToken cancellationToken);
}
