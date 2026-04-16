using Invoria.Procurement.Contracts.Models;

namespace Invoria.Procurement.Contracts.Events;

public sealed class PurchaseOrderCompletedIntegrationEvent
{
    public required string PurchaseOrderId { get; set; }
    public required string PurchaseNumber { get; set; }
    public required string SupplierId { get; set; }
    public required DateTimeOffset CompletedAt { get; set; }
    public required List<PurchaseOrderItemModel> Items { get; set; }
}

