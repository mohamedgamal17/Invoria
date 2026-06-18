using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published when a processing allocated order moves to revision pending.
/// Carries the order snapshot plus the allocation to release in inventory.
/// </summary>
public class OrderRevisionRequestedIntegrationEvent
{
    public required OrderModel Order { get; set; }

    public required string AllocationId { get; set; }

    public required DateTimeOffset OccurredOn { get; set; }
}
