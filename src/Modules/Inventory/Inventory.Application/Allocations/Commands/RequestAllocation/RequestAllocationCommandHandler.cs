using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Services;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;

public sealed class RequestAllocationCommandHandler
    : IApplicatonRequestHandler<RequestAllocationCommand, Empty>
{
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryRepository<Allocation> _allocationRepository;
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IAllocationDomainService _allocationDomainService;

    public RequestAllocationCommandHandler(
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
        RequestAllocationCommand request,
        CancellationToken cancellationToken)
    {
        var allocationId = request.AllocationId;

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var allocation = await _allocationRepository.Single(
                a => a.Id == allocationId,
                cancellationToken);

            var batchesByProduct = await LoadFifoBatchesByProductAsync(allocation, cancellationToken);
            _allocationDomainService.Allocate(allocation, batchesByProduct);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<Empty>(
                new InvalidOperationException($"Request allocation failed: {ex.Message}", ex));
        }
    }

    private async Task<Dictionary<string, List<Batch>>> LoadFifoBatchesByProductAsync(
        Allocation allocation,
        CancellationToken cancellationToken)
    {
        var batches = await _batchRepository.AsQuerable()
            .Where(b => allocation.Lines.Select(l => l.ProductId).Contains(b.ProductId) && b.State == BatchState.Active)
            .OrderBy(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);

        return batches
            .GroupBy(b => b.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
