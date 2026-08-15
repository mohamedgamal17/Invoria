namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published by the OrderSaga when an order is completed to request the allocation
/// consumption snapshot (allocation lines and batch allocations with quantities and unit prices).
/// </summary>
public sealed class RequestOrderAllocationIntegrationEvent
{
    public required string OrderId { get; set; }

    public required string AllocationId { get; set; }
}
