namespace Invoria.Ordering.Contracts.Events;

/// <summary>
/// Published when an order is refused without a two-phase release (e.g. not allocated yet).
/// </summary>
public class OrderRefusedIntegrationEvent
{
    public required string OrderId { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public DateTimeOffset RefusedAt { get; set; }
}
