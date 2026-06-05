using Invoria.Inventory.Contracts.Allocations.Enums;

namespace Invoria.Inventory.Contracts.Allocations.Models;

/// <summary>
/// Shared allocation snapshot for integration messaging.
/// </summary>
public class AllocationModel
{
    public required string Id { get; set; }

    public required string OrderId { get; set; }

    public AllocationStatus Status { get; set; }

    public required List<AllocationLineModel> Lines { get; set; }
}
