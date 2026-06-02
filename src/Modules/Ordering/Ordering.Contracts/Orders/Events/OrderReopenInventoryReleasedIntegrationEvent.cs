namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published by Inventory after allocations for a reopening order have been released (or there were none).
/// Ordering completes reopen: <see cref="OrderId"/> is the order aggregate id.
/// </summary>
public class OrderReopenInventoryReleasedIntegrationEvent
{
    public required string OrderId { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public DateTimeOffset ReleasedAt { get; set; }
}
