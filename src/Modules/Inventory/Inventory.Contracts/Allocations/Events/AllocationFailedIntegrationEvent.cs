namespace Invoria.Inventory.Contracts.Allocations.Events;
public sealed class AllocationFailedIntegrationEvent
{
    public required string AllocationId { get; set; }

    public required string OrderId { get; set; }
}
