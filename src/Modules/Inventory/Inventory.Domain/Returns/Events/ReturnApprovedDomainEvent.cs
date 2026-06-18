using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Returns.Events;

public class ReturnApprovedDomainEvent : DomainEvent
{
    public ReturnApprovedDomainEvent(Return @return)
    {
        Return = Guard.Against.Null(@return);
    }

    public Return Return { get; }
}
