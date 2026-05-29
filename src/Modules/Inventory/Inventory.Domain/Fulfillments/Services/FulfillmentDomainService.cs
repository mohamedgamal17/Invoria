using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain.Allocations;

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
}
