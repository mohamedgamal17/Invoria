namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published by Inventory after allocations for a <strong>refused</strong> order have been released (or there were none).
/// </summary>
public class OrderRefusalInventoryReleasedIntegrationEvent
{
    public required string OrderId { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public DateTimeOffset ReleasedAt { get; set; }
}
