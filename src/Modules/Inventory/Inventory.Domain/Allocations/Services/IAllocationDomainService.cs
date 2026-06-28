using Invoria.BuildingBlocks.Domain.Services;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Domain.Allocations.Services;

public interface IAllocationDomainService : IDomainService
{
    void Allocate(
        Allocation allocation,
        IReadOnlyDictionary<string, List<Batch>> batchesByProduct);

    void Release(
        Allocation allocation,
        IReadOnlyDictionary<string, Batch> batchesById);

    void Complete(
        Allocation allocation,
        IReadOnlyDictionary<string, Batch> batchesById);
}
