using Invoria.Ordering.Contracts.Models;

namespace Invoria.Ordering.Contracts.Events;

/// <summary>
/// Published when inventory should release batch allocations for order lines (reopen or refusal).
/// Use <see cref="ReleaseReason"/> to distinguish completion flow. <see cref="Id"/> is the order aggregate id.
/// </summary>
public class ReleaseOrderAllocationsIntegrationEvent
{
    public required string Id { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public required List<OrderItemModel> Items { get; set; }

    /// <summary>Default <see cref="AllocationReleaseReason.Reopen"/> for backward compatibility.</summary>
    public AllocationReleaseReason ReleaseReason { get; set; } = AllocationReleaseReason.Reopen;
}
