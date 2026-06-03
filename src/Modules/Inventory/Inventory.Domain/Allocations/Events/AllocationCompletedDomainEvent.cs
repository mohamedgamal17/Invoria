using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Allocations.Events;

/// <summary>
/// Raised when every allocation line was fully reserved from batches.
/// </summary>
public class AllocationCompletedDomainEvent : DomainEvent
{
    public AllocationCompletedDomainEvent(Allocation allocation)
    {
        Allocation = Guard.Against.Null(allocation);
    }

    public Allocation Allocation { get; }
}
