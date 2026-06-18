namespace Invoria.Inventory.Contracts.Allocations.Models;

/// <summary>
/// Order line payload for allocate-order integration messaging.
/// </summary>
public class AllocateOrderLineModel
{
    public required string Id { get; set; }

    public required string ProductId { get; set; }

    public int Quantity { get; set; }
}
