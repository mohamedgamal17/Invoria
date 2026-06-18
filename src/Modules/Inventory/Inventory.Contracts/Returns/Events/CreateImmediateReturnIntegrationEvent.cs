using Invoria.Inventory.Contracts.Returns.Models;

namespace Invoria.Inventory.Contracts.Returns.Events;

public class CreateImmediateReturnIntegrationEvent
{
    public string OrderId { get; set; } = null!;

    public string AllocationId { get; set; } = null!;

    public List<ReturnLineModel> Lines { get; set; } = [];
}
