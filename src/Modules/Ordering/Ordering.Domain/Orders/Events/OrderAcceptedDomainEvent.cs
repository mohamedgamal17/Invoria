using Invoria.BuildingBlocks.Domain.Events;
using MediatR;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed class OrderAcceptedDomainEvent : DomainEvent, INotification
{
    public OrderAcceptedDomainEvent(string orderId, string orderNumber, string customerId)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
    }

    public string OrderId { get; }
    public string OrderNumber { get; }
    public string CustomerId { get; }
}
