using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Batches.Commands.DispatchOrder;

public sealed class DispatchOrderCommandHandler
    : IApplicatonRequestHandler<DispatchOrderCommand, Empty>
{
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IInventoryRepository<BatchAllocation> _allocationRepository;
    private readonly IInventoryRepository<OrderDispatchProcessed> _processedRepository;

    public DispatchOrderCommandHandler(
        IInventoryRepository<Batch> batchRepository,
        IInventoryRepository<BatchAllocation> allocationRepository,
        IInventoryRepository<OrderDispatchProcessed> processedRepository)
    {
        _batchRepository = batchRepository;
        _allocationRepository = allocationRepository;
        _processedRepository = processedRepository;
    }

    public async Task<Result<Empty>> Handle(
        DispatchOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            return Result.Success(Empty.Value);
        }

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return Result.Failure<Empty>(new InvalidOperationException("Order id is required for dispatch inventory processing."));
        }

        var alreadyProcessed = await _processedRepository.SingleOrDefault(
            p => p.Id == request.Id,
            cancellationToken);
        if (alreadyProcessed != null)
        {
            return Result.Success(Empty.Value);
        }

        var linesWithStock = request.Items.Where(i => i.Quantity > 0).ToList();
        if (linesWithStock.Count == 0)
        {
            return Result.Success(Empty.Value);
        }

        var demandByOrderItem = linesWithStock
            .GroupBy(i => i.Id)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var orderItemIds = demandByOrderItem.Keys.ToList();

        var allocations = await _allocationRepository.AsQuerable()
            .Include(a => a.Batch)
            .Where(a => orderItemIds.Contains(a.OrderItemId))
            .ToListAsync(cancellationToken);

        foreach (var (orderItemId, expectedQty) in demandByOrderItem)
        {
            var allocatedSum = allocations
                .Where(a => a.OrderItemId == orderItemId)
                .Sum(a => a.QuantityAllocated);

            if (allocatedSum != expectedQty)
            {
                return Result.Failure<Empty>(new InvalidOperationException(
                    $"Allocated quantity ({allocatedSum}) does not match dispatched quantity ({expectedQty}) for order item {orderItemId}."));
            }
        }

        try
        {
            foreach (var allocation in allocations)
            {
                var batch = allocation.Batch
                    ?? throw new InvalidOperationException($"Batch {allocation.BatchId} was not loaded for allocation {allocation.Id}.");

                batch.ReleaseReservedForDispatch(allocation.QuantityAllocated);
                await _batchRepository.Update(batch, cancellationToken);
            }

            await _processedRepository.Add(
                new OrderDispatchProcessed(request.Id, DateTimeOffset.UtcNow),
                cancellationToken);

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Empty>(new InvalidOperationException($"Dispatch inventory processing failed: {ex.Message}", ex));
        }
    }
}
