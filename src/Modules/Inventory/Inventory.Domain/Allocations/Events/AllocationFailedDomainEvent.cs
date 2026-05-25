using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Allocations.Events;

/// <summary>
/// Raised when the allocation could not be fully satisfied across all lines.
/// </summary>
public sealed class AllocationFailedDomainEvent : DomainEvent
{
    private AllocationFailedDomainEvent(string allocationId, string orderId)
    {
        Guard.Against.NullOrWhiteSpace(allocationId);
        Guard.Against.OutOfRange(allocationId.Length, nameof(allocationId), 1, AllocationTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, AllocationTableConsts.OrderIdMaxLength);

        AllocationId = allocationId;
        OrderId = orderId;
    }

    public string AllocationId { get; }

    public string OrderId { get; }

    public static AllocationFailedDomainEvent ForAllocation(Allocation allocation)
    {
        if (allocation.Status != AllocationStatus.Failed)
        {
            throw new InvalidOperationException(
                $"Allocation {allocation.Id} must be in {AllocationStatus.Failed} state.");
        }

        return new AllocationFailedDomainEvent(allocation.Id!, allocation.OrderId);
    }
}
