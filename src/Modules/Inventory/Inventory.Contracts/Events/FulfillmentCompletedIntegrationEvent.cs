namespace Invoria.Inventory.Contracts.Events;

/// <summary>
/// Published when fulfillment dispatch completes and the fulfillment is marked completed.
/// </summary>
public sealed class FulfillmentCompletedIntegrationEvent
{
    public required string FulfillmentId { get; set; }

    public required string OrderId { get; set; }

    public required string AllocationId { get; set; }
}
