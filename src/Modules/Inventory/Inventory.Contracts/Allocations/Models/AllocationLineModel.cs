using Invoria.Inventory.Contracts.Allocations.Enums;

namespace Invoria.Inventory.Contracts.Allocations.Models;

public class AllocationLineModel
{
    public required string Id { get; set; }

    public required string OrderItemId { get; set; }

    public required string ProductId { get; set; }

    public int QuantityRequested { get; set; }

    public AllocationLineStatus Status { get; set; }
}
