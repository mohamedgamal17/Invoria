using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Reporting.Domain.Orders.DebtSummary;

/// <summary>
/// Materialized debt rollup shared shape, rebuilt periodically from completed <see cref="ReportedOrder"/>
/// rows with outstanding balance.
/// </summary>
public abstract class DebtSummaryBase : IBaseEntity
{
    public string Id { get; set; } = null!;

    public DebtSummaryType SummaryType { get; set; }

    public decimal TotalOutstanding { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal TotalOrderValue { get; set; }

    public int DebtOrderCount { get; set; }

    public int PartiallyPaidCount { get; set; }

    public int UnpaidCount { get; set; }

    public DateTimeOffset ComputedAt { get; set; }
}
