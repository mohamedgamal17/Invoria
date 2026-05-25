namespace Invoria.Inventory.Contracts.Events;

/// <summary>
/// Published when the allocation could not be fully satisfied across all lines.
/// </summary>
public sealed class AllocationFailedIntegrationEvent
{
    public required string AllocationId { get; set; }

    public required string OrderId { get; set; }
}
