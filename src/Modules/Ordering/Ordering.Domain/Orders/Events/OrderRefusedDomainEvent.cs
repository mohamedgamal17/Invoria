using Invoria.BuildingBlocks.Domain.Events;
using MediatR;

namespace Invoria.Ordering.Domain.Orders.Events;

/// <summary>
/// Raised when an order is refused without a two-phase inventory release (e.g. not yet allocated).
/// </summary>
public sealed class OrderRefusedDomainEvent : DomainEvent, INotification
{
    public OrderRefusedDomainEvent(string orderId, string orderNumber, string customerId)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
    }

    public string OrderId { get; }
    public string OrderNumber { get; }
    public string CustomerId { get; }
}
