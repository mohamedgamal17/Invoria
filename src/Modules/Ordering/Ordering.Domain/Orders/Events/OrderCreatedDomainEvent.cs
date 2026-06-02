using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed class OrderCreatedDomainEvent : DomainEvent
{
    public OrderCreatedDomainEvent(Order order)
    {
        Order = Guard.Against.Null(order);
    }

    public Order Order { get; }
}
