using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Domain.Services;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Domain.Fulfillments.Services;

public interface IFulfillmentDomainService : IDomainService
{
    Result<Fulfillment> CreateFulfillment(Allocation allocation);

    Result<Empty> Dispatch(
        Fulfillment fulfillment,
        Allocation allocation,
        IReadOnlyDictionary<string, Batch> batchesById);
}
