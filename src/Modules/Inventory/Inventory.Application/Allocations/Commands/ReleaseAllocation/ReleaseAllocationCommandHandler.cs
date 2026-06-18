using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Services;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Allocations.Commands.ReleaseAllocation;

public sealed class ReleaseAllocationCommandHandler
    : IApplicatonRequestHandler<ReleaseAllocationCommand, Empty>
{
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryRepository<Allocation> _allocationRepository;
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IAllocationDomainService _allocationDomainService;

    public ReleaseAllocationCommandHandler(
        IInventoryUnitOfWork unitOfWork,
        IInventoryRepository<Allocation> allocationRepository,
        IInventoryRepository<Batch> batchRepository,
        IAllocationDomainService allocationDomainService)
    {
        _unitOfWork = unitOfWork;
        _allocationRepository = allocationRepository;
        _batchRepository = batchRepository;
        _allocationDomainService = allocationDomainService;
    }

    public async Task<Result<Empty>> Handle(
        ReleaseAllocationCommand request,
        CancellationToken cancellationToken)
    {
        var allocationId = request.AllocationId;

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var allocation = await _allocationRepository.Single(
                a => a.Id == allocationId,
                cancellationToken);

            var batchesById = await LoadBatchesForReleaseAsync(allocation, cancellationToken);
            _allocationDomainService.Release(allocation, batchesById);

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

    private async Task<Dictionary<string, Batch>> LoadBatchesForReleaseAsync(
        Allocation allocation,
        CancellationToken cancellationToken)
    {
        var batchIds = allocation.Lines
            .SelectMany(l => l.BatchAllocations)
            .Select(a => a.BatchId)
            .Distinct();

        var batches = await _batchRepository.AsQuerable()
            .Where(b => batchIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        return batches.ToDictionary(b => b.Id!);
    }
}
