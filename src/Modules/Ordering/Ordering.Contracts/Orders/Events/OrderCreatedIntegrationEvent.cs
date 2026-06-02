using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published when a new sales order is persisted.
/// Carries the order snapshot in <see cref="Order"/> plus when the event was raised.
/// </summary>
public class OrderCreatedIntegrationEvent
{
    public required OrderIntegrationPayload Order { get; set; }

    public required DateTimeOffset OccurredOn { get; set; }
}
