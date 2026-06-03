namespace Invoria.Inventory.Contracts.Allocations.Events;

/// <summary>
/// Published when stock reserved for an allocation should be released.
/// </summary>
public sealed class ReleaseAllocationIntegrationEvent
{
    public required string AllocationId { get; set; }
}
