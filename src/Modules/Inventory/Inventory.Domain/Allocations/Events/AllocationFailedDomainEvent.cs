using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Allocations.Events;

/// <summary>
/// Raised when the allocation could not be fully satisfied across all lines.
/// </summary>
public class AllocationFailedDomainEvent : DomainEvent
{
    public AllocationFailedDomainEvent(Allocation allocation)
    {
        Allocation = Guard.Against.Null(allocation);
    }

    public Allocation Allocation { get; }
}
