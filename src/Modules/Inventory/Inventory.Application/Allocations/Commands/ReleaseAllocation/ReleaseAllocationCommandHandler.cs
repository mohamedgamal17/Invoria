using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Allocations.Commands.ReleaseAllocation;

public sealed class ReleaseAllocationCommandHandler
    : IApplicatonRequestHandler<ReleaseAllocationCommand, Empty>
{
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryRepository<Allocation> _allocationRepository;
    private readonly IInventoryRepository<Batch> _batchRepository;

    public ReleaseAllocationCommandHandler(
        IInventoryUnitOfWork unitOfWork,
        IInventoryRepository<Allocation> allocationRepository,
        IInventoryRepository<Batch> batchRepository)
    {
        _unitOfWork = unitOfWork;
        _allocationRepository = allocationRepository;
        _batchRepository = batchRepository;
    }

    public async Task<Result<Empty>> Handle(
        ReleaseAllocationCommand request,
        CancellationToken cancellationToken)
    {
        var allocationId = request.AllocationId;

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var allocation = await _allocationRepository.AsQuerable()
                .Include(a => a.Lines)
                .ThenInclude(l => l.BatchAllocations)
                .AsSplitQuery()
                .SingleAsync(a => a.Id == allocationId, cancellationToken);

            if (allocation.Status == AllocationStatus.Released)
            {
                await transaction.CommitAsync(cancellationToken);
                return Result.Success(Empty.Value);
            }

            var batchAllocationsToRelease = allocation.Lines
                .Where(l => l.Status == AllocationLineStatus.Allocated)
                .SelectMany(l => l.BatchAllocations)
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .ToList();

            foreach (var group in batchAllocationsToRelease.GroupBy(a => a.BatchId))
            {
                var batch = await _batchRepository.AsQuerable()
                    .SingleAsync(b => b.Id == group.Key, cancellationToken);

                batch.RestoreAllocatedQuantity(group.Sum(a => a.QuantityAllocated));
            }

            allocation.MarkAsReleased();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<Empty>(
                new InvalidOperationException($"Release allocation failed: {ex.Message}", ex));
        }
    }
}
