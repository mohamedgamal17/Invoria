using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;

public sealed class RequestAllocationCommandHandler
    : IApplicatonRequestHandler<RequestAllocationCommand, Empty>
{
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryRepository<Allocation> _allocationRepository;
    private readonly IInventoryRepository<Batch> _batchRepository;

    public RequestAllocationCommandHandler(
        IInventoryUnitOfWork unitOfWork,
        IInventoryRepository<Allocation> allocationRepository,
        IInventoryRepository<Batch> batchRepository)
    {
        _unitOfWork = unitOfWork;
        _allocationRepository = allocationRepository;
        _batchRepository = batchRepository;
    }

    public async Task<Result<Empty>> Handle(
        RequestAllocationCommand request,
        CancellationToken cancellationToken)
    {
        var allocationId = request.AllocationId;

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var allocation = await _allocationRepository.AsQuerable()
                .Include(a => a.Lines)
                .ThenInclude(l => l.BatchAllocations)
                .SingleAsync(a => a.Id == allocationId, cancellationToken);

            var lines = allocation.Lines
                .Where(l => l.Status == AllocationLineStatus.Pending)
                .ToList();

            if (lines.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return Result.Success(Empty.Value);
            }

            var batchesByProduct = await LoadFifoBatchesByProductAsync(lines, cancellationToken);
            var batchesById = ToBatchesById(batchesByProduct);
            var allocatedAt = DateTimeOffset.UtcNow;

            foreach (var line in lines)
            {
                if (!batchesByProduct.TryGetValue(line.ProductId, out var batches))
                {
                    line.MarkAsFailed();
                    continue;
                }

                var remaining = line.QuantityRequested;

                foreach (var batch in batches)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var take = Math.Min(remaining, batch.Quantity);
                    if (take <= 0)
                    {
                        continue;
                    }

                    var batchAllocation = batch.AllocateForOrder(line.OrderItemId, take, allocatedAt);
                    line.RecordBatchAllocation(batchAllocation);
                    remaining -= take;
                }

                if (line.IsFullyAllocated)
                {
                    line.MarkAsAllocated();
                }
                else
                {
                    line.MarkAsFailed();
                }
            }

            if (!allocation.TryMarkAsAllocated())
            {
                allocation.MarkAsFailed();

                foreach (var line in allocation.Lines)
                {
                    if (line.BatchAllocations.Count == 0)
                    {
                        continue;
                    }

                    ReleaseLineStock(line, batchesById);

                    if (line.Status != AllocationLineStatus.Failed)
                    {
                        line.MarkAsReleased();
                    }
                }
            }

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

    private static void ReleaseLineStock(AllocationLine line, IReadOnlyDictionary<string, Batch> batchesById)
    {
        foreach (var batchAllocation in line.BatchAllocations)
        {
            batchesById[batchAllocation.BatchId].RestoreAllocatedQuantity(batchAllocation.QuantityAllocated);
        }
    }

    private static Dictionary<string, Batch> ToBatchesById(
        IReadOnlyDictionary<string, List<Batch>> batchesByProduct) =>
        batchesByProduct.Values
            .SelectMany(b => b)
            .ToDictionary(b => b.Id!);

    private async Task<Dictionary<string, List<Batch>>> LoadFifoBatchesByProductAsync(
        IReadOnlyList<AllocationLine> lines,
        CancellationToken cancellationToken)
    {
        var productIds = lines.Select(l => l.ProductId).Distinct().ToList();

        var batches = await _batchRepository.AsQuerable()
            .Where(b => productIds.Contains(b.ProductId) && b.State == BatchState.Active)
            .OrderBy(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);

        return batches
            .GroupBy(b => b.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
