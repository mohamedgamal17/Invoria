using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Batches.Commands.ReleaseOrderAllocations;

public sealed class ReleaseOrderAllocationsCommandHandler
    : IApplicatonRequestHandler<ReleaseOrderAllocationsCommand, Empty>
{
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IInventoryRepository<BatchAllocation> _allocationRepository;

    public ReleaseOrderAllocationsCommandHandler(
        IInventoryRepository<Batch> batchRepository,
        IInventoryRepository<BatchAllocation> allocationRepository)
    {
        _batchRepository = batchRepository;
        _allocationRepository = allocationRepository;
    }

    public async Task<Result<Empty>> Handle(
        ReleaseOrderAllocationsCommand request,
        CancellationToken cancellationToken)
    {
        var orderItemIds = request.Items
            .Select(i => i.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (orderItemIds.Count == 0)
        {
            return Result.Success(Empty.Value);
        }

        var allocations = await _allocationRepository.AsQuerable()
            .Include(a => a.Batch)
            .Where(a => orderItemIds.Contains(a.OrderItemId))
            .ToListAsync(cancellationToken);

        try
        {
            foreach (var allocation in allocations)
            {
                var batch = allocation.Batch
                    ?? throw new InvalidOperationException(
                        $"Batch was not loaded for allocation {allocation.Id}.");

                batch.RestoreAllocatedQuantity(allocation.QuantityAllocated);
                await _batchRepository.Update(batch, cancellationToken);
                await _allocationRepository.Delete(allocation, cancellationToken);
            }

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Empty>(
                new InvalidOperationException($"Release order allocations failed: {ex.Message}", ex));
        }
    }
}
