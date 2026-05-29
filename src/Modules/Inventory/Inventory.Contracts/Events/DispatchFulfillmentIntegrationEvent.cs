namespace Invoria.Inventory.Contracts.Events;

/// <summary>
/// Published when fulfillment dispatch is requested and the fulfillment is in progress.
/// </summary>
public sealed class DispatchFulfillmentIntegrationEvent
{
    public required string FulfillmentId { get; set; }

    public required string OrderId { get; set; }

    public required string AllocationId { get; set; }
}
