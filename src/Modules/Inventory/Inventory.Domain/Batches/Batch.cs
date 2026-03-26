using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Batches;

public class Batch : AuditedAggregateRoot
{
    public string ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public decimal PurchasePrice { get; private set; }

    // for ef core
    private Batch()
    {
    }

    public Batch(string productId, int quantity, int reservedQuantity, decimal purchasePrice)
    {
        Guard.Against.NullOrWhiteSpace(productId);
        Guard.Against.OutOfRange(productId.Length, nameof(productId), 1, BatchTableConsts.ProductIdMaxLength);
        Guard.Against.Negative(quantity);
        Guard.Against.Negative(reservedQuantity);
        Guard.Against.NegativeOrZero(purchasePrice);

        ProductId = productId;
        Quantity = quantity;
        ReservedQuantity = reservedQuantity;
        PurchasePrice = purchasePrice;
    }
}
