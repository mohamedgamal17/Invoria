using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Domain.Fulfillments.Services;

public sealed class FulfillmentDomainService : IFulfillmentDomainService
{
    public Result<Fulfillment> CreateFulfillment(Allocation allocation)
    {
        Guard.Against.Null(allocation);

        if (allocation.Status != AllocationStatus.Allocated)
        {
            return Result.Failure<Fulfillment>(
                new BusinessLogicException(
                    $"Fulfillment can only be created from an allocation in {AllocationStatus.Allocated} state."));
        }

        return Result.Success(Fulfillment.CreateFromAllocation(allocation));
    }

    public Result<Empty> Dispatch(
        Fulfillment fulfillment,
        Allocation allocation,
        IReadOnlyDictionary<string, Batch> batchesById)
    {
        foreach (var line in allocation.Lines)
        {
            foreach (var batchAllocation in line.BatchAllocations)
            {
                batchesById[batchAllocation.BatchId]
                    .DispatchReservedQuantity(batchAllocation.QuantityAllocated);
            }
        }

        fulfillment.Complete();

        return Result.Success(Empty.Value);
    }
}
