namespace Invoria.Financial.Contracts.Payables.Events;

public class CreatePurchaseOrderPayableIntegrationEvent
{
    public string PurchaseOrderId { get; set; } = null!;

    public string SupplierId { get; set; } = null!;

    public decimal Amount { get; set; }
}
