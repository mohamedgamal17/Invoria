using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Domain.Returns.Services;

public sealed class ReturnDomainService : IReturnDomainService
{
    public void ProcessImmediateReturn(
        ImmediateReturn immediateReturn,
        Allocation allocation,
        IReadOnlyList<Batch> batches)
    {
        var allocationLinesByOrderItemId = allocation.Lines.ToDictionary(l => l.OrderItemId);

        var batchesById = batches.ToDictionary(b => b.Id!);

        foreach (var returnLine in immediateReturn.ReturnLines)
        {
            var allocationLine = allocationLinesByOrderItemId[returnLine.OrderItemId];

            var remaining = returnLine.Quantity;

            foreach (var batchAllocation in allocationLine.BatchAllocations
                .OrderByDescending(ba => ba.BatchId))
            {
                var batchReturn = Math.Min(remaining, batchAllocation.QuantityAllocated);

                var batch = batchesById[batchAllocation.BatchId];

                batch.AddReturn(batchReturn);

                remaining -= batchReturn;

                if (remaining <= 0)
                {
                    break;
                }
            }
        }

        immediateReturn.Complete();
    }
}
