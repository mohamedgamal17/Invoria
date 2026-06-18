using Invoria.Inventory.Contracts.Allocations.Models;

namespace Invoria.Inventory.Contracts.Allocations.Events;

public class AllocateOrderIntegrationEvent
{
    public required string Id { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public required List<AllocateOrderLineModel> Items { get; set; }
}
