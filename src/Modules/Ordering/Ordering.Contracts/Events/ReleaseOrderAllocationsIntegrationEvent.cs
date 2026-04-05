using Invoria.Ordering.Contracts.Models;

namespace Invoria.Ordering.Contracts.Events;

/// <summary>
/// Published when an accepted order is reopening and inventory should release batch allocations for the order lines.
/// <see cref="Id"/> is the order aggregate id.
/// </summary>
public class ReleaseOrderAllocationsIntegrationEvent
{
    public required string Id { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public required List<OrderItemModel> Items { get; set; }
}
