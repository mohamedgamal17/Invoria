namespace Invoria.Inventory.Contracts.Allocations.Events;

/// <summary>
/// Published when every allocation line was fully reserved from batches.
/// </summary>
public sealed class AllocationSucceededIntegrationEvent
{
    public required string AllocationId { get; set; }

    public required string OrderId { get; set; }
}
