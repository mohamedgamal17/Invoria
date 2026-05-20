using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;

public sealed record StatusChunkAggregate(
    DateOnly DayUtc,
    OrderStatus OrderStatus,
    int Count);
