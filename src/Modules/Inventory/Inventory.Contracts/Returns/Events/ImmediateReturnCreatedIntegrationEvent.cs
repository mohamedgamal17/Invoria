namespace Invoria.Inventory.Contracts.Returns.Events;

public class ImmediateReturnCreatedIntegrationEvent
{
    public string ReturnId { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public string AllocationId { get; set; } = null!;
}
