using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Domain.Allocations;

public class BatchAllocation : AuditedEntity
{
    public string BatchId { get; private set; } = null!;
    public string OrderItemId { get; private set; } = null!;
    public int QuantityAllocated { get; private set; }
    public DateTimeOffset AllocatedAt { get; private set; }
    public string? AllocationLineId { get; private set; }

    public Batch? Batch { get; private set; }
    public AllocationLine? AllocationLine { get; private set; }

    private BatchAllocation()
    {
    }

    public BatchAllocation(string batchId, string orderItemId, int quantityAllocated, DateTimeOffset allocatedAt)
    {
        Guard.Against.NullOrWhiteSpace(batchId);
        Guard.Against.OutOfRange(batchId.Length, nameof(batchId), 1, BatchAllocationTableConsts.BatchIdMaxLength);
        Guard.Against.NullOrWhiteSpace(orderItemId);
        Guard.Against.OutOfRange(orderItemId.Length, nameof(orderItemId), 1, BatchAllocationTableConsts.OrderItemIdMaxLength);
        Guard.Against.NegativeOrZero(quantityAllocated);

        BatchId = batchId;
        OrderItemId = orderItemId;
        QuantityAllocated = quantityAllocated;
        AllocatedAt = allocatedAt;
    }
}
