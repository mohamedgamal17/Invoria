using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Domain.Orders;

public class OrderStateTransitionHistory : Entity
{
    public string OrderId { get; private set; } = null!;

    public OrderStatus FromStatus { get; private set; }

    public OrderStatus ToStatus { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    private OrderStateTransitionHistory()
    {
    }

    public OrderStateTransitionHistory(
        string id,
        string orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        DateTimeOffset changedAt)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.OutOfRange(id.Length, nameof(id), 1, OrderStateTransitionHistoryTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, OrderStateTransitionHistoryTableConsts.OrderIdMaxLength);

        Id = id;
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedAt = changedAt;
    }
}