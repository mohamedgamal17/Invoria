using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Fulfillments;
using Invoria.Inventory.Domain.Fulfillments.Services;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Fulfillments.Commands.DispatchFulfillment;

public sealed class DispatchFulfillmentCommandHandler
    : IApplicatonRequestHandler<DispatchFulfillmentCommand, Empty>
{
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryRepository<Fulfillment> _fulfillmentRepository;
    private readonly IInventoryRepository<Allocation> _allocationRepository;
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IInventoryRepository<BatchAllocation> _batchAllocationRepository;
    private readonly IFulfillmentDomainService _fulfillmentDomainService;

    public DispatchFulfillmentCommandHandler(
        IInventoryUnitOfWork unitOfWork,
        IInventoryRepository<Fulfillment> fulfillmentRepository,
        IInventoryRepository<Allocation> allocationRepository,
        IInventoryRepository<Batch> batchRepository,
        IInventoryRepository<BatchAllocation> batchAllocationRepository,
        IFulfillmentDomainService fulfillmentDomainService)
    {
        _unitOfWork = unitOfWork;
        _fulfillmentRepository = fulfillmentRepository;
        _allocationRepository = allocationRepository;
        _batchRepository = batchRepository;
        _batchAllocationRepository = batchAllocationRepository;
        _fulfillmentDomainService = fulfillmentDomainService;
    }

    public async Task<Result<Empty>> Handle(
        DispatchFulfillmentCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var fulfillmentId = request.FulfillmentId;

            var fulfillment = await _fulfillmentRepository.SingleOrDefault(
                f => f.Id == fulfillmentId,
                cancellationToken);

            if (fulfillment is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<Empty>(
                    new NotFoundException($"Fulfillment with ID {fulfillmentId} not found"));
            }

            if (fulfillment.Status != FulfillmentStatus.InProgress)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<Empty>(
                    new BusinessLogicException(
                        $"Fulfillment {fulfillmentId} must be in {FulfillmentStatus.InProgress} state to dispatch."));
            }

            var allocation = await _allocationRepository.AsQuerable()
                .IgnoreAutoIncludes()
                .Include(a => a.Lines)
                .SingleAsync(a => a.Id == fulfillment.AllocationId, cancellationToken);

            await HydrateBatchAllocationsAsync(allocation, cancellationToken);

            var batchesById = await LoadBatchesForDispatchAsync(allocation, cancellationToken);
            var dispatchResult = _fulfillmentDomainService.Dispatch(fulfillment, allocation, batchesById);

            if (dispatchResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<Empty>(dispatchResult.Exception!);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<Empty>(
                new InvalidOperationException($"Dispatch fulfillment failed: {ex.Message}", ex));
        }
    }

    private async Task HydrateBatchAllocationsAsync(
        Allocation allocation,
        CancellationToken cancellationToken)
    {
        var lineIds = allocation.Lines.Select(l => l.Id).ToList();

        var batchAllocations = await _batchAllocationRepository.AsQuerable()
            .Where(ba => ba.AllocationLineId != null && lineIds.Contains(ba.AllocationLineId))
            .ToListAsync(cancellationToken);

        foreach (var line in allocation.Lines)
        {
            var forLine = batchAllocations.Where(ba => ba.AllocationLineId == line.Id);
            line.SetBatchAllocations(forLine);
        }
    }

    private async Task<Dictionary<string, Batch>> LoadBatchesForDispatchAsync(
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
