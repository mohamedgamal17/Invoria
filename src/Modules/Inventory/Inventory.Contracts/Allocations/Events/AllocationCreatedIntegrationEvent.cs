using Invoria.Inventory.Contracts.Allocations.Models;

namespace Invoria.Inventory.Contracts.Allocations.Events;

/// <summary>
/// Published when a pending allocation aggregate is created for an order.
/// Carries the allocation snapshot in <see cref="Allocation"/> plus when the event was raised.
/// </summary>
public class AllocationCreatedIntegrationEvent
{
    public required AllocationModel Allocation { get; set; }

    public required DateTimeOffset OccurredOn { get; set; }
}
