using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Contracts.Events;

namespace Invoria.Inventory.Application.Batches.Commands.CreateBatchesFromPurchaseOrderCompleted;

public sealed class CreateBatchesFromPurchaseOrderCompletedCommand : ICommand<Empty>
{
    public string PurchaseOrderId { get; init; } = string.Empty;
    public string PurchaseNumber { get; init; } = string.Empty;
    public string SupplierId { get; init; } = string.Empty;
    public DateTimeOffset CompletedAt { get; init; }
    public List<Item> Items { get; init; } = [];

    public sealed class Item
    {
        public string PurchaseOrderItemId { get; init; } = string.Empty;
        public string ProductId { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }

    public static CreateBatchesFromPurchaseOrderCompletedCommand FromEvent(PurchaseOrderCompletedIntegrationEvent message) =>
        new()
        {
            PurchaseOrderId = message.PurchaseOrderId,
            PurchaseNumber = message.PurchaseNumber,
            SupplierId = message.SupplierId,
            CompletedAt = message.CompletedAt,
            Items = message.Items
                .Select(i => new Item
                {
                    PurchaseOrderItemId = i.PurchaseOrderItemId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList()
        };
}
