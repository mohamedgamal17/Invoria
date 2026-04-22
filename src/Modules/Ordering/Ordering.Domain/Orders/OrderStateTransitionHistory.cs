using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Ordering.Domain.Orders;

public class OrderStateTransitionHistory : Entity
{
    public string OrderId { get; private set; } = null!;
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public FullfillmentStatus FromFullfillmentStatus { get; private set; }
    public FullfillmentStatus ToFullfillmentStatus { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string? Reason { get; private set; }

    private OrderStateTransitionHistory()
    {
    }

    internal OrderStateTransitionHistory(
        string orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        FullfillmentStatus fromFullfillmentStatus,
        FullfillmentStatus toFullfillmentStatus,
        DateTimeOffset changedAt,
        string? reason)
    {
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, OrderStateTransitionHistoryTableConsts.OrderIdMaxLength);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Guard.Against.OutOfRange(
                reason.Length,
                nameof(reason),
                1,
                OrderStateTransitionHistoryTableConsts.ReasonMaxLength);
        }

        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        FromFullfillmentStatus = fromFullfillmentStatus;
        ToFullfillmentStatus = toFullfillmentStatus;
        ChangedAt = changedAt;
        Reason = reason;
    }
}
