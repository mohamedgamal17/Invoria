using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Procurement.Domain.PurchaseOrders.Events;

public sealed class PurchaseOrderCompletedDomainEvent : DomainEvent
{
    public PurchaseOrderCompletedDomainEvent(
        string purchaseOrderId,
        string purchaseNumber,
        string supplierId,
        DateTimeOffset completedAt,
        decimal totalAmount,
        IReadOnlyList<Item> items)
    {
        PurchaseOrderId = purchaseOrderId;
        PurchaseNumber = purchaseNumber;
        SupplierId = supplierId;
        CompletedAt = completedAt;
        TotalAmount = totalAmount;
        Items = items;
    }

    public string PurchaseOrderId { get; }
    public string PurchaseNumber { get; }
    public string SupplierId { get; }
    public DateTimeOffset CompletedAt { get; }
    public decimal TotalAmount { get; }
    public IReadOnlyList<Item> Items { get; }

    public sealed record Item(
        string PurchaseOrderItemId,
        string ProductId,
        int Quantity,
        decimal UnitPrice);
}

