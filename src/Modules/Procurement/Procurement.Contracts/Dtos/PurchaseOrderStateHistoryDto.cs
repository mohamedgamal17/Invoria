using Invoria.Procurement.Contracts.PurchaseOrders;

namespace Invoria.Procurement.Contracts.Dtos;

public sealed class PurchaseOrderStateHistoryDto
{
    public PurchaseState FromState { get; set; }

    public PurchaseState ToState { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public string? Reason { get; set; }
}
