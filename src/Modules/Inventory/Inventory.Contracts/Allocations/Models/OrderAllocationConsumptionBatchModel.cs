namespace Invoria.Inventory.Contracts.Allocations.Models;

/// <summary>
/// A batch allocation snapshot within an order allocation consumption,
/// carrying the allocated quantity and the batch unit price.
/// </summary>
public class OrderAllocationConsumptionBatchModel
{
    public required string BatchId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
