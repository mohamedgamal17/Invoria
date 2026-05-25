using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Allocations.Events;

/// <summary>
/// Raised when every allocation line was fully reserved from batches.
/// </summary>
public sealed class AllocationCompletedDomainEvent : DomainEvent
{
    private AllocationCompletedDomainEvent(string allocationId, string orderId)
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

    public static AllocationCompletedDomainEvent ForAllocation(Allocation allocation)
    {
        if (allocation.Status != AllocationStatus.Allocated)
        {
            throw new InvalidOperationException(
                $"Allocation {allocation.Id} must be in {AllocationStatus.Allocated} state.");
        }

        return new AllocationCompletedDomainEvent(allocation.Id!, allocation.OrderId);
    }
}
