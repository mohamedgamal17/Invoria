namespace Invoria.Inventory.Contracts.Allocations.Events;

public sealed class AllocationReleasedIntegrationEvent
{
    public required string AllocationId { get; set; }

    public required string OrderId { get; set; }
}
