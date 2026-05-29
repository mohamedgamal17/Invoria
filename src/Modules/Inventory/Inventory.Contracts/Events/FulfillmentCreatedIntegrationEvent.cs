namespace Invoria.Inventory.Contracts.Events;

/// <summary>
/// Published when a fulfillment is created from an allocated allocation.
/// </summary>
public sealed class FulfillmentCreatedIntegrationEvent
{
    public required string FulfillmentId { get; set; }

    public required string OrderId { get; set; }

    public required string AllocationId { get; set; }
}
