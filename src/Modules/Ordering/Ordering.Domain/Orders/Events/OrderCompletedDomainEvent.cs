using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed class OrderCompletedDomainEvent : DomainEvent
{
    public OrderCompletedDomainEvent(Order order)
    {
        Order = Guard.Against.Null(order);
        OrderId = order.Id;
    }

    public string OrderId { get; }

    public Order Order { get; }
}
