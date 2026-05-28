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
            .Where(a => orderItemIds.Contains(a.OrderItemId))
            .ToListAsync(cancellationToken);

        if (allocations.Count == 0)
        {
            return Result.Success(Empty.Value);
        }

        var batchesById = await LoadBatchesByIdAsync(allocations, cancellationToken);

        try
        {
            foreach (var allocation in allocations)
            {
                var batch = batchesById[allocation.BatchId];
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

    private async Task<Dictionary<string, Batch>> LoadBatchesByIdAsync(
        IReadOnlyList<BatchAllocation> allocations,
        CancellationToken cancellationToken)
    {
        var batchIds = allocations.Select(a => a.BatchId).Distinct().ToList();

        var batches = await _batchRepository.AsQuerable()
            .Where(b => batchIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        return batches.ToDictionary(b => b.Id!);
    }
}
