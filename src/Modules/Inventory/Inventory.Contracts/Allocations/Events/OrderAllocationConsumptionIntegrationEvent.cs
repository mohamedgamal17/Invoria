using Invoria.Inventory.Contracts.Allocations.Models;

namespace Invoria.Inventory.Contracts.Allocations.Events;

/// <summary>
/// Published by Inventory in response to a request order allocation to carry the
/// allocation consumption snapshot (lines and batch allocations with quantity and unit prices).
/// </summary>
public sealed class OrderAllocationConsumptionIntegrationEvent
{
    public required string OrderId { get; set; }

    public required string AllocationId { get; set; }

    public required List<OrderAllocationConsumptionLineModel> Lines { get; set; }
}
