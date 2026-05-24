using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Inventory.Domain.Allocations.Events;

/// <summary>
/// Raised when a pending allocation is initiated and batch reservation should proceed.
/// </summary>
public sealed class AllocationInitiatedDomainEvent : DomainEvent
{
    private AllocationInitiatedDomainEvent(string allocationId, AllocationStatus status)
    {
        Guard.Against.NullOrWhiteSpace(allocationId);
        Guard.Against.OutOfRange(allocationId.Length, nameof(allocationId), 1, AllocationTableConsts.IdMaxLength);

        if (status != AllocationStatus.Pending)
        {
            throw new ArgumentException(
                $"Status must be {AllocationStatus.Pending}.",
                nameof(status));
        }

        AllocationId = allocationId;
        Status = status;
    }

    public string AllocationId { get; }
    public AllocationStatus Status { get; }

    public static AllocationInitiatedDomainEvent ForPendingAllocation(Allocation allocation)
    {
        if (allocation.Status != AllocationStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Allocation {allocation.Id} must be in {AllocationStatus.Pending} state to initiate.");
        }

        return new AllocationInitiatedDomainEvent(allocation.Id!, AllocationStatus.Pending);
    }
}
