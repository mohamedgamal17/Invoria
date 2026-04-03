using Invoria.Ordering.Contracts.Models;

namespace Invoria.Ordering.Contracts.Events;

/// <summary>
/// Published when an order should be allocated against inventory batches.
/// <see cref="Id"/> is the order aggregate id.
/// </summary>
public class AllocateOrderIntegrationEvent
{
    public required string Id { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public required List<OrderItemModel> Items { get; set; }
}
