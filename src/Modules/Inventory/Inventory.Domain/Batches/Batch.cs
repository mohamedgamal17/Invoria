using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Batches;

public class Batch : AuditedAggregateRoot
{
    public string ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public BatchState State { get; private set; }

    // for ef core
    private Batch()
    {
    }

    public Batch(string productId, int quantity,  decimal purchasePrice)
    {
        Guard.Against.NullOrWhiteSpace(productId);
        Guard.Against.OutOfRange(productId.Length, nameof(productId), 1, BatchTableConsts.ProductIdMaxLength);
        Guard.Against.Negative(quantity);
        Guard.Against.NegativeOrZero(purchasePrice);

        ProductId = productId;
        Quantity = quantity;
        PurchasePrice = purchasePrice;
        State = quantity > 0 ? BatchState.Active : BatchState.Depleted;
    }
    public void UpdateQuantity(int quantity)
    {
        Guard.Against.Negative(quantity);

        Quantity = quantity;

        if (State == BatchState.Disabled)
        {
            return;
        }

        State = quantity > 0 ? BatchState.Active : BatchState.Depleted;
    }

    public void UpdatePurchasePrice(decimal purchasePrice)
    {
        Guard.Against.NegativeOrZero(purchasePrice);

        PurchasePrice = purchasePrice;
    }

    public void Disable()
    {
        if (State != BatchState.Active)
        {
            throw new InvalidOperationException("Batch can only be disabled when it is Active.");
        }

        State = BatchState.Disabled;
    }

    public void Enable()
    {
        if (State != BatchState.Disabled)
        {
            throw new InvalidOperationException("Batch can only be enabled when it is Disabled.");
        }

        if (Quantity <= 0)
        {
            throw new InvalidOperationException("Batch cannot be enabled when it has zero quantity.");
        }

        State = BatchState.Active;
    }
}
