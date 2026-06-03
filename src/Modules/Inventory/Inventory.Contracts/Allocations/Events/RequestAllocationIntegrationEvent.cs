namespace Invoria.Inventory.Contracts.Allocations.Events;

/// <summary>
/// Published when batch reservation should proceed for a pending allocation.
/// </summary>
public sealed class RequestAllocationIntegrationEvent
{
    public required string AllocationId { get; set; }
}
