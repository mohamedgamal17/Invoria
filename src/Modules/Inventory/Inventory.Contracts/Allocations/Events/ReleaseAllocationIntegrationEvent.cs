namespace Invoria.Inventory.Contracts.Allocations.Events;

public sealed class ReleaseAllocationIntegrationEvent
{
    public required string AllocationId { get; set; }
}
