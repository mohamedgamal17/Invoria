using Invoria.BuildingBlocks.Domain.Services;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Domain.Returns.Services;

public interface IReturnDomainService : IDomainService
{
    void ProcessImmediateReturn(
        ImmediateReturn immediateReturn,
        Allocation allocation,
        IReadOnlyList<Batch> batches);
}
