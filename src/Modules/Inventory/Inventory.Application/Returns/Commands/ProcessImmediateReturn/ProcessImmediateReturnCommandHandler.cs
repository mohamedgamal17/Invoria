using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Domain.Returns.Services;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Returns.Commands.ProcessImmediateReturn;

public class ProcessImmediateReturnCommandHandler
    : IApplicatonRequestHandler<ProcessImmediateReturnCommand, Empty>
{
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryRepository<ImmediateReturn> _immediateReturnRepository;
    private readonly IInventoryRepository<Allocation> _allocationRepository;
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IReturnDomainService _returnDomainService;

    public ProcessImmediateReturnCommandHandler(
        IInventoryUnitOfWork unitOfWork,
        IInventoryRepository<ImmediateReturn> immediateReturnRepository,
        IInventoryRepository<Allocation> allocationRepository,
        IInventoryRepository<Batch> batchRepository,
        IReturnDomainService returnDomainService)
    {
        _unitOfWork = unitOfWork;
        _immediateReturnRepository = immediateReturnRepository;
        _allocationRepository = allocationRepository;
        _batchRepository = batchRepository;
        _returnDomainService = returnDomainService;
    }

    public async Task<Result<Empty>> Handle(
        ProcessImmediateReturnCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var immediateReturn = await _immediateReturnRepository.Single(
                r => r.Id == request.ReturnId,
                cancellationToken);

            var allocation = await _allocationRepository.Single(
                a => a.Id == immediateReturn.AllocationId,
                cancellationToken);

            var batches = await LoadBatchesForAllocationAsync(allocation, cancellationToken);
            _returnDomainService.ProcessImmediateReturn(immediateReturn, allocation, batches);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<Empty>(
                new InvalidOperationException($"Process immediate return failed: {ex.Message}", ex));
        }
    }

    private async Task<List<Batch>> LoadBatchesForAllocationAsync(
        Allocation allocation,
        CancellationToken cancellationToken)
    {
        var batchIds = allocation.Lines
            .SelectMany(l => l.BatchAllocations)
            .Select(a => a.BatchId)
            .Distinct()
            .ToList();

        return await _batchRepository.AsQuerable()
            .Where(b => batchIds.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }
}
