using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Domain.Allocations.Services;

public sealed class AllocationDomainService : IAllocationDomainService
{
    public void Allocate(
        Allocation allocation,
        IReadOnlyDictionary<string, List<Batch>> batchesByProduct)
    {
        var batchesById = ToBatchesById(batchesByProduct);
        var allocatedAt = DateTimeOffset.UtcNow;

        foreach (var line in allocation.Lines)
        {
            if (!batchesByProduct.TryGetValue(line.ProductId, out var batches))
            {
                line.MarkAsFailed();
                continue;
            }

            var remaining = line.QuantityRequested;

            foreach (var batch in batches)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var take = Math.Min(remaining, batch.Quantity);
                if (take <= 0)
                {
                    continue;
                }

                var batchAllocation = batch.AllocateForOrder(line.OrderItemId, take, allocatedAt);
                line.RecordBatchAllocation(batchAllocation);
                remaining -= take;
            }

            if (line.IsFullyAllocated)
            {
                line.MarkAsAllocated();
            }
            else
            {
                line.MarkAsFailed();
            }
        }

        if (!allocation.TryMarkAsAllocated())
        {
            allocation.MarkAsFailed();

            foreach (var line in allocation.Lines)
            {
                if (line.BatchAllocations.Count == 0)
                {
                    continue;
                }

                ReleaseLineStock(line, batchesById);

                if (line.Status != AllocationLineStatus.Failed)
                {
                    line.MarkAsReleased();
                }
            }
        }
    }

    private static void ReleaseLineStock(AllocationLine line, IReadOnlyDictionary<string, Batch> batchesById)
    {
        foreach (var batchAllocation in line.BatchAllocations)
        {
            batchesById[batchAllocation.BatchId].RestoreAllocatedQuantity(batchAllocation.QuantityAllocated);
        }
    }

    private static Dictionary<string, Batch> ToBatchesById(
        IReadOnlyDictionary<string, List<Batch>> batchesByProduct) =>
        batchesByProduct.Values
            .SelectMany(b => b)
            .ToDictionary(b => b.Id!);
}
