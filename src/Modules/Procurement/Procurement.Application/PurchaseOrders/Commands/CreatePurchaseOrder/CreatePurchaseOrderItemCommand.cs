namespace Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderItemCommand
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? SupplierProductCode { get; set; }

    public CreatePurchaseOrderItemCommand(
        string productId,
        int quantity,
        decimal unitPrice,
        string? supplierProductCode)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        SupplierProductCode = supplierProductCode;
    }
}
