using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Allocations.Extensions;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Allocations.Queries.GetOrderAllocationConsumption;

public sealed class GetOrderAllocationConsumptionQueryHandler
    : IApplicatonRequestHandler<GetOrderAllocationConsumptionQuery, OrderAllocationConsumptionIntegrationEvent>
{
    private readonly IInventoryRepository<Allocation> _allocationRepository;
    private readonly IInventoryRepository<Batch> _batchRepository;

    public GetOrderAllocationConsumptionQueryHandler(
        IInventoryRepository<Allocation> allocationRepository,
        IInventoryRepository<Batch> batchRepository)
    {
        _allocationRepository = allocationRepository;
        _batchRepository = batchRepository;
    }

    public async Task<Result<OrderAllocationConsumptionIntegrationEvent>> Handle(
        GetOrderAllocationConsumptionQuery request,
        CancellationToken cancellationToken)
    {
        var allocationId = request.AllocationId;

        var allocation = await _allocationRepository.SingleOrDefault(
            a => a.Id == allocationId,
            cancellationToken);

        if (allocation is null)
        {
            return Result.Failure<OrderAllocationConsumptionIntegrationEvent>(
                new NotFoundException($"Allocation {allocationId} was not found."));
        }

        var batchesById = await LoadBatchesByIdAsync(allocation, cancellationToken);
        var consumption = allocation.ToOrderAllocationConsumptionIntegrationEvent(batchesById);

        return Result.Success(consumption);
    }

    private async Task<IReadOnlyDictionary<string, Batch>> LoadBatchesByIdAsync(
        Allocation allocation,
        CancellationToken cancellationToken)
    {
        var batchIds = allocation.Lines
            .SelectMany(line => line.BatchAllocations)
            .Select(batchAllocation => batchAllocation.BatchId)
            .Distinct();

        var batches = await _batchRepository.AsQuerable()
            .Where(batch => batchIds.Contains(batch.Id))
            .ToListAsync(cancellationToken);

        return batches.ToDictionary(batch => batch.Id!);
    }
}
