using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Allocations;

public class BatchAllocation : AuditedEntity
{
    public string BatchId { get; private set; } = null!;
    public string OrderItemId { get; private set; } = null!;
    public int QuantityAllocated { get; private set; }
    public DateTimeOffset AllocatedAt { get; private set; }
    public string? AllocationLineId { get; private set; }

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

    public void AttachToLine(AllocationLine line)
    {
        Guard.Against.Null(line);
        Guard.Against.NullOrWhiteSpace(line.Id);

        if (!string.Equals(OrderItemId, line.OrderItemId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Batch allocation order item must match the allocation line order item.");
        }

        AllocationLineId = line.Id;
    }
}
