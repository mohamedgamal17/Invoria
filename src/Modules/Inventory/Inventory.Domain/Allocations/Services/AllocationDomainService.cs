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

    public void Release(
        Allocation allocation,
        IReadOnlyDictionary<string, Batch> batchesById)
    {
        if (allocation.Status == AllocationStatus.Released)
        {
            return;
        }

        var batchAllocationsToRelease = allocation.Lines
            .Where(l => l.Status == AllocationLineStatus.Allocated)
            .SelectMany(l => l.BatchAllocations)
            .GroupBy(a => a.Id)
            .Select(g => g.First())
            .ToList();

        foreach (var group in batchAllocationsToRelease.GroupBy(a => a.BatchId))
        {
            batchesById[group.Key].RestoreAllocatedQuantity(group.Sum(a => a.QuantityAllocated));
        }

        allocation.MarkAsReleased();
    }

    private void ReleaseLineStock(AllocationLine line, IReadOnlyDictionary<string, Batch> batchesById)
    {
        foreach (var batchAllocation in line.BatchAllocations)
        {
            batchesById[batchAllocation.BatchId].RestoreAllocatedQuantity(batchAllocation.QuantityAllocated);
        }
    }

    private Dictionary<string, Batch> ToBatchesById(
        IReadOnlyDictionary<string, List<Batch>> batchesByProduct) =>
        batchesByProduct.Values
            .SelectMany(b => b)
            .ToDictionary(b => b.Id!);
}
