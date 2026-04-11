using Invoria.BuildingBlocks.Domain.Events;
using MediatR;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed record OrderAcceptedLine(string OrderItemId, string ProductId, int Quantity);

public sealed class OrderAcceptedDomainEvent : DomainEvent, INotification
{
    public OrderAcceptedDomainEvent(
        string orderId,
        string orderNumber,
        string customerId,
        IReadOnlyList<OrderAcceptedLine> lines)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Lines = lines;
    }

    public string OrderId { get; }
    public string OrderNumber { get; }
    public string CustomerId { get; }
    public IReadOnlyList<OrderAcceptedLine> Lines { get; }
}
