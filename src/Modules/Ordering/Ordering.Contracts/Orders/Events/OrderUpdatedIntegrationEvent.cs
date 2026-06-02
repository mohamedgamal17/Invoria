using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published when a persisted sales order aggregate is updated.
/// Carries the order snapshot in <see cref="Order"/> plus when the event was raised.
/// </summary>
public class OrderUpdatedIntegrationEvent
{
    public required OrderIntegrationPayload Order { get; set; }

    public required DateTimeOffset OccurredOn { get; set; }
}
