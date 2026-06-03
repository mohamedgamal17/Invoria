using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published when a sales order is accepted and moves to processing.
/// Carries the order snapshot in <see cref="Order"/> plus when the event was raised.
/// </summary>
public class OrderAcceptedIntegrationEvent
{
    public required OrderModel Order { get; set; }

    public required DateTimeOffset OccurredOn { get; set; }
}
