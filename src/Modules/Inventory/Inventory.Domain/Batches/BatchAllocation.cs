using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Batches;

public class BatchAllocation : AuditedEntity
{
    public string BatchId { get; private set; }
    public string OrderItemId { get; private set; }
    public int QuantityAllocated { get; private set; }
    public DateTimeOffset AllocatedAt { get; private set; }

    public Batch? Batch { get; private set; }

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
