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
    public int AvailableQuantity => Quantity;

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

    /// <summary>
    /// Reserves stock for an order line and records which batch fulfilled it.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the batch is not active or stock is insufficient.</exception>
    public BatchAllocation AllocateForOrder(string orderItemId, int amount, DateTimeOffset allocatedAt)
    {
        Guard.Against.NullOrWhiteSpace(orderItemId);
        Guard.Against.OutOfRange(orderItemId.Length, nameof(orderItemId), 1, BatchAllocationTableConsts.OrderItemIdMaxLength);
        Guard.Against.NegativeOrZero(amount);

        if (State != BatchState.Active)
        {
            throw new InvalidOperationException("Stock can only be allocated from an active batch.");
        }

        if (Quantity < amount)
        {
            throw new InvalidOperationException("Insufficient available quantity in this batch.");
        }

        if (string.IsNullOrEmpty(Id))
        {
            throw new InvalidOperationException("Batch must have an identifier before allocations can be recorded.");
        }

        Quantity -= amount;
        ReservedQuantity += amount;

        if (Quantity == 0)
        {
            State = BatchState.Depleted;
        }

        var allocation = new BatchAllocation(Id, orderItemId, amount, allocatedAt);
        return allocation;
    }

    /// <summary>
    /// Releases reserved stock when the corresponding order line is dispatched (shipped).
    /// Does not change on-hand <see cref="Quantity"/>; that was reduced at allocation time.
    /// </summary>
    public void ReleaseReservedForDispatch(int amount)
    {
        Guard.Against.NegativeOrZero(amount);

        if (ReservedQuantity < amount)
        {
            throw new InvalidOperationException(
                "Cannot release more reserved quantity than is currently reserved on this batch.");
        }

        ReservedQuantity -= amount;
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
