using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Reporting.Domain.Orders;

/// <summary>
/// Materialized aggregate: count of reported orders per UTC calendar day of creation and current order status.
/// Rebuilt periodically from <see cref="ReportedOrder"/>; not an auditable entity.
/// </summary>
public sealed class ReportedOrderStatusByDay
{
    public DateOnly DayUtc { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public int Count { get; set; }
}
