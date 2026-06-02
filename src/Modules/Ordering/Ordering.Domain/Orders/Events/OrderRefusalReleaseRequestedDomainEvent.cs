using Invoria.BuildingBlocks.Domain.Events;
using MediatR;

namespace Invoria.Ordering.Domain.Orders.Events;

/// <summary>
/// Raised when refusing an order requires inventory to release batch allocations; application layer translates
/// to a release integration event marked for refusal.
/// </summary>
public sealed class OrderRefusalReleaseRequestedDomainEvent : DomainEvent, INotification
{
    public OrderRefusalReleaseRequestedDomainEvent(
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
