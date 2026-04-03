using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Batches.Commands.AllocateOrder;

public sealed class AllocateOrderCommandHandler
    : IApplicatonRequestHandler<AllocateOrderCommand, Empty>
{
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IInventoryRepository<BatchAllocation> _allocationRepository;

    public AllocateOrderCommandHandler(
        IInventoryRepository<Batch> batchRepository,
        IInventoryRepository<BatchAllocation> allocationRepository)
    {
        _batchRepository = batchRepository;
        _allocationRepository = allocationRepository;
    }

    public async Task<Result<Empty>> Handle(
        AllocateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var preFlightErrors = await GetPreFlightErrorsAsync(request, cancellationToken);
        if (preFlightErrors.Count != 0)
        {
            return Result.Failure<Empty>(new OrderAllocationPreFlightException(preFlightErrors));
        }

        try
        {
            foreach (var item in request.Items)
            {
                var remaining = item.Quantity;
                while (remaining > 0)
                {
                    var batches = await _batchRepository.AsQuerable()
                        .Where(b => b.ProductId == item.ProductId && b.State == BatchState.Active)
                        .OrderBy(b => b.Id)
                        .ToListAsync(cancellationToken);

                    var batch = batches.FirstOrDefault(b => b.AvailableQuantity > 0);
                    if (batch == null)
                    {
                        return Result.Failure<Empty>(new InvalidOperationException(
                            "Allocation failed: available stock changed during processing. Retry the operation."));
                    }

                    var take = Math.Min(remaining, batch.AvailableQuantity);
                    var allocation = batch.AllocateForOrder(item.Id, take, DateTimeOffset.UtcNow);
                    await _batchRepository.Update(batch, cancellationToken);
                    await _allocationRepository.Add(allocation, cancellationToken);
                    remaining -= take;
                }
            }

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Empty>(new InvalidOperationException($"Allocation failed: {ex.Message}", ex));
        }
    }

    private async Task<List<OrderAllocationPreFlightError>> GetPreFlightErrorsAsync(
        AllocateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var demandByProduct = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
        var errors = new List<OrderAllocationPreFlightError>();

        foreach (var (productId, demand) in demandByProduct)
        {
            var available = await _batchRepository.AsQuerable()
                .AsNoTracking()
                .Where(b => b.ProductId == productId && b.State == BatchState.Active)
                .SumAsync(b => b.Quantity, cancellationToken);

            if (available < demand)
            {
                errors.Add(new OrderAllocationPreFlightError(
                    productId,
                    demand,
                    available));
            }
        }

        return errors;
    }
}
