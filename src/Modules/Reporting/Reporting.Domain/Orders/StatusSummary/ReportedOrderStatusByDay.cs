using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Reporting.Domain.Orders.StatusSummary;

/// <summary>
/// Materialized aggregate: count of reported orders per UTC calendar day of creation and current order status.
/// Rebuilt periodically from <see cref="Invoria.Reporting.Domain.Orders.ReportedOrder"/>; not an auditable entity.
/// </summary>
public sealed class ReportedOrderStatusByDay : IBaseEntity
{
    public DateOnly DayUtc { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public int Count { get; set; }
}
