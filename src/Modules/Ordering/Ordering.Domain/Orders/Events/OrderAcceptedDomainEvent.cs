using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;
using Invoria.Ordering.Domain.Orders;
using MediatR;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed class OrderAcceptedDomainEvent : DomainEvent, INotification
{
    public OrderAcceptedDomainEvent(Order order)
    {
        Order = Guard.Against.Null(order);
    }

    public Order Order { get; }
}
