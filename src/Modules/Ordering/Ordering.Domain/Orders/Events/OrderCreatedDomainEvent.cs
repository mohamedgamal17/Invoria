using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed class OrderCreatedDomainEvent : DomainEvent
{
    public OrderCreatedDomainEvent(string orderId, string orderNumber, string customerId)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
    }

    public string OrderId { get; }
    public string OrderNumber { get; }
    public string CustomerId { get; }
}
