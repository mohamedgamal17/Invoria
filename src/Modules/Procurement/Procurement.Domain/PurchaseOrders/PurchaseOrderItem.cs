using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Procurement.Domain.PurchaseOrders;

public class PurchaseOrderItem : Entity
{
#pragma warning disable CS0414
    private string _legacyCreatedBatchIds = "[]";
#pragma warning restore CS0414

    public string PurchaseOrderId { get; private set; } = null!;

    public string ProductId { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public string? SupplierProductCode { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

    private PurchaseOrderItem()
    {
    }

    public PurchaseOrderItem(
        string id,
        string purchaseOrderId,
        string productId,
        int quantity,
        decimal unitPrice,
        string? supplierProductCode = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(purchaseOrderId))
        {
            throw new ArgumentException("Purchase order id cannot be empty.", nameof(purchaseOrderId));
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException("Product id cannot be empty.", nameof(productId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        Id = id;
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        SupplierProductCode = supplierProductCode;
    }
}
