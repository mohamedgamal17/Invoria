using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed class OrderDispatchedDomainEvent : DomainEvent
{
    public OrderDispatchedDomainEvent(
        string orderId,
        string orderNumber,
        string customerId,
        IReadOnlyList<OrderDispatchedLine> lines)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Lines = lines;
    }

    public string OrderId { get; }
    public string OrderNumber { get; }
    public string CustomerId { get; }
    public IReadOnlyList<OrderDispatchedLine> Lines { get; }
}

public sealed record OrderDispatchedLine(string OrderItemId, string ProductId, int Quantity);
