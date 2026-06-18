using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Returns.Events;

public class ImmediateReturnCreatedDomainEvent : DomainEvent
{
    public ImmediateReturnCreatedDomainEvent(ImmediateReturn immediateReturn)
    {
        ImmediateReturn = Guard.Against.Null(immediateReturn);
    }

    public ImmediateReturn ImmediateReturn { get; }
}
