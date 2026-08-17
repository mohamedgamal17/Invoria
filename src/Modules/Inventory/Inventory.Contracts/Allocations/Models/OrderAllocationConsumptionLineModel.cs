namespace Invoria.Inventory.Contracts.Allocations.Models;

/// <summary>
/// An allocation line snapshot within an order allocation consumption,
/// carrying its fulfilled batch allocations.
/// </summary>
public class OrderAllocationConsumptionLineModel
{
    public required string OrderItemId { get; set; }

    public required string ProductId { get; set; }

    public int QuantityRequested { get; set; }

    public required List<OrderAllocationConsumptionBatchModel> BatchAllocations { get; set; }
}
