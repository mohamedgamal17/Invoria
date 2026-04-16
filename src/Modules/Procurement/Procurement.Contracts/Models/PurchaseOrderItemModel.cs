namespace Invoria.Procurement.Contracts.Models;

public sealed class PurchaseOrderItemModel
{
    public required string PurchaseOrderItemId { get; set; }
    public required string ProductId { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public string? SupplierProductCode { get; set; }
}

