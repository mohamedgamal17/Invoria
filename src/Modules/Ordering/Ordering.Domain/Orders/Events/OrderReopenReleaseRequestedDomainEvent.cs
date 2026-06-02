using Invoria.BuildingBlocks.Domain.Events;
using MediatR;

namespace Invoria.Ordering.Domain.Orders.Events;

public sealed record OrderDispatchedLine(string OrderItemId, string ProductId, int Quantity);

/// <summary>
/// Raised when reopening requires inventory to release allocations; application layer can translate to an integration event.
/// </summary>
public sealed class OrderReopenReleaseRequestedDomainEvent : DomainEvent, INotification
{
    public OrderReopenReleaseRequestedDomainEvent(
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
