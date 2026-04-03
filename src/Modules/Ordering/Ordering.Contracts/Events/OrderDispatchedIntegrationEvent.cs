using Invoria.Ordering.Contracts.Models;

namespace Invoria.Ordering.Contracts.Events;

/// <summary>
/// Published when an order is marked dispatched (fulfillment shipping).
/// <see cref="Id"/> is the order aggregate id.
/// </summary>
public class OrderDispatchedIntegrationEvent
{
    public required string Id { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public required DateTimeOffset DispatchedAt { get; set; }

    public required List<OrderItemModel> Items { get; set; }
}
