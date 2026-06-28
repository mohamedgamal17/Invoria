namespace Invoria.Inventory.Contracts.Allocations.Events;

public sealed class AllocationSettledIntegrationEvent
{
    public required string AllocationId { get; set; }

    public required string OrderId { get; set; }
}
