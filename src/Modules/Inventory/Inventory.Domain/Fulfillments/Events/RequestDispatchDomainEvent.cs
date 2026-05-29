using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Fulfillments.Events;

/// <summary>
/// Raised when dispatch is requested for a pending fulfillment.
/// </summary>
public sealed class RequestDispatchDomainEvent : DomainEvent
{
    private RequestDispatchDomainEvent(string fulfillmentId, string orderId, string allocationId)
    {
        Guard.Against.NullOrWhiteSpace(fulfillmentId);
        Guard.Against.OutOfRange(fulfillmentId.Length, nameof(fulfillmentId), 1, FulfillmentTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, FulfillmentTableConsts.OrderIdMaxLength);
        Guard.Against.NullOrWhiteSpace(allocationId);
        Guard.Against.OutOfRange(allocationId.Length, nameof(allocationId), 1, FulfillmentTableConsts.AllocationIdMaxLength);

        FulfillmentId = fulfillmentId;
        OrderId = orderId;
        AllocationId = allocationId;
    }

    public string FulfillmentId { get; }

    public string OrderId { get; }

    public string AllocationId { get; }

    public static RequestDispatchDomainEvent ForFulfillment(Fulfillment fulfillment)
    {
        Guard.Against.Null(fulfillment);

        return new RequestDispatchDomainEvent(
            fulfillment.Id!,
            fulfillment.OrderId,
            fulfillment.AllocationId);
    }
}
