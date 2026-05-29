using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Domain.Services;
using Invoria.Inventory.Domain.Allocations;

namespace Invoria.Inventory.Domain.Fulfillments.Services;

public interface IFulfillmentDomainService : IDomainService
{
    Result<Fulfillment> CreateFulfillment(Allocation allocation);
}
