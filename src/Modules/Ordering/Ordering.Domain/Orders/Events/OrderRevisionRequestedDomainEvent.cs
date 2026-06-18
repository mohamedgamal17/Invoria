using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;
using Invoria.Ordering.Domain.Orders;
using MediatR;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed class OrderRevisionRequestedDomainEvent : DomainEvent, INotification
{
    public OrderRevisionRequestedDomainEvent(Order order)
    {
        Order = Guard.Against.Null(order);
    }

    public Order Order { get; }
}
