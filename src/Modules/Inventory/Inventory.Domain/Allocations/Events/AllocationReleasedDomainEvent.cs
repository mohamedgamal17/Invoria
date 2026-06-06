using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Allocations.Events;

/// <summary>
/// Raised when an allocated allocation is released back to inventory.
/// </summary>
public class AllocationReleasedDomainEvent : DomainEvent
{
    public AllocationReleasedDomainEvent(Allocation allocation)
    {
        Allocation = Guard.Against.Null(allocation);
    }

    public Allocation Allocation { get; }
}
