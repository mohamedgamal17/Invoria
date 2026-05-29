using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Allocations;

public class AllocationLine : AuditedEntity
{
    public string AllocationId { get; private set; } = null!;

    public string OrderItemId { get; private set; } = null!;

    public string ProductId { get; private set; } = null!;

    public int QuantityRequested { get; private set; }

    public AllocationLineStatus Status { get; private set; }

    public ICollection<BatchAllocation> BatchAllocations { get; private set; } = null!;

    private AllocationLine()
    {
        BatchAllocations = new HashSet<BatchAllocation>();
    }

    public AllocationLine(
        string id,
        string allocationId,
        string orderItemId,
        string productId,
        int quantityRequested)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.OutOfRange(id.Length, nameof(id), 1, AllocationLineTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(allocationId);
        Guard.Against.OutOfRange(allocationId.Length, nameof(allocationId), 1, AllocationLineTableConsts.AllocationIdMaxLength);
        Guard.Against.NullOrWhiteSpace(orderItemId);
        Guard.Against.OutOfRange(orderItemId.Length, nameof(orderItemId), 1, AllocationLineTableConsts.OrderItemIdMaxLength);
        Guard.Against.NullOrWhiteSpace(productId);
        Guard.Against.OutOfRange(productId.Length, nameof(productId), 1, AllocationLineTableConsts.ProductIdMaxLength);
        Guard.Against.NegativeOrZero(quantityRequested);

        Id = id;
        AllocationId = allocationId;
        OrderItemId = orderItemId;
        ProductId = productId;
        QuantityRequested = quantityRequested;
        Status = AllocationLineStatus.Pending;
        BatchAllocations = new HashSet<BatchAllocation>();
    }

    public int QuantityAllocated => BatchAllocations.Sum(a => a.QuantityAllocated);

    public bool IsFullyAllocated => QuantityAllocated == QuantityRequested;

    public void RecordBatchAllocation(BatchAllocation batchAllocation)
    {
        Guard.Against.Null(batchAllocation);

        if (Status != AllocationLineStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Allocation line {Id} must be in {AllocationLineStatus.Pending} state to record batch allocations.");
        }

        batchAllocation.AttachToLine(this);
        BatchAllocations.Add(batchAllocation);
    }

    public void SetBatchAllocations(IEnumerable<BatchAllocation> batchAllocations)
    {
        BatchAllocations.Clear();
        foreach (var batchAllocation in batchAllocations)
        {
            BatchAllocations.Add(batchAllocation);
        }
    }

    public void MarkAsAllocated()
    {
        if (Status != AllocationLineStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Allocation line {Id} must be in {AllocationLineStatus.Pending} state to mark as allocated.");
        }

        if (!IsFullyAllocated)
        {
            throw new InvalidOperationException(
                $"Allocation line {Id} is not fully allocated: {QuantityAllocated} of {QuantityRequested}.");
        }

        Status = AllocationLineStatus.Allocated;
    }

    public void MarkAsFailed()
    {
        if (Status != AllocationLineStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Allocation line {Id} must be in {AllocationLineStatus.Pending} state to mark as failed.");
        }

        if (IsFullyAllocated)
        {
            throw new InvalidOperationException(
                $"Allocation line {Id} is fully allocated and cannot be marked as failed.");
        }

        Status = AllocationLineStatus.Failed;
    }

    public void MarkAsReleased()
    {
        if (Status != AllocationLineStatus.Allocated)
        {
            throw new InvalidOperationException(
                $"Allocation line {Id} must be in {AllocationLineStatus.Allocated} state to mark as released.");
        }

        Status = AllocationLineStatus.Released;
    }
}
