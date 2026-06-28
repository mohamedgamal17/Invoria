using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Allocations.Events;

public class AllocationSettledDomainEvent : DomainEvent
{
    public AllocationSettledDomainEvent(Allocation allocation)
    {
        Allocation = Guard.Against.Null(allocation);
    }

    public Allocation Allocation { get; }
}
