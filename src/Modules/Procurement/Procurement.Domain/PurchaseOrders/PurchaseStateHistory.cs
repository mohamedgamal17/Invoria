using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Procurement.Domain.PurchaseOrders;

public class PurchaseStateHistory : Entity
{
    public string PurchaseOrderId { get; private set; } = null!;

    public PurchaseState FromState { get; private set; }

    public PurchaseState ToState { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public string? Reason { get; private set; }

    private PurchaseStateHistory()
    {
    }

    internal PurchaseStateHistory(
        string id,
        string purchaseOrderId,
        PurchaseState fromState,
        PurchaseState toState,
        DateTimeOffset changedAt,
        string? reason)
    {
        Id = id;
        PurchaseOrderId = purchaseOrderId;
        FromState = fromState;
        ToState = toState;
        ChangedAt = changedAt;
        Reason = reason;
    }
}
