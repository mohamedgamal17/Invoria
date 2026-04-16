using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Procurement.Domain.PurchaseOrders.Events;

public sealed class PurchaseOrderCompletedDomainEvent : DomainEvent
{
    public PurchaseOrderCompletedDomainEvent(
        string purchaseOrderId,
        string purchaseNumber,
        string supplierId,
        DateTimeOffset completedAt,
        IReadOnlyList<Item> items)
    {
        PurchaseOrderId = purchaseOrderId;
        PurchaseNumber = purchaseNumber;
        SupplierId = supplierId;
        CompletedAt = completedAt;
        Items = items;
    }

    public string PurchaseOrderId { get; }
    public string PurchaseNumber { get; }
    public string SupplierId { get; }
    public DateTimeOffset CompletedAt { get; }
    public IReadOnlyList<Item> Items { get; }

    public sealed record Item(
        string PurchaseOrderItemId,
        string ProductId,
        int Quantity,
        decimal UnitPrice,
        string? SupplierProductCode);
}

