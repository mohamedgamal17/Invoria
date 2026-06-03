using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Allocations.Events;

public class AllocationInitiatedDomainEvent : DomainEvent
{
    public AllocationInitiatedDomainEvent(Allocation allocation)
    {
        Allocation = Guard.Against.Null(allocation);
    }

    public Allocation Allocation { get; }
}
