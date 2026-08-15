using ContractAllocationLineStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationLineStatus;
using ContractAllocationStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationStatus;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Allocations.Extensions;

public static class AllocationMappingExtensions
{
    public static AllocationModel ToAllocationModel(this Allocation allocation) => new()
    {
        Id = allocation.Id!,
        OrderId = allocation.OrderId,
        Status = (ContractAllocationStatus)allocation.Status,
        Lines = allocation.Lines.Select(l => l.ToAllocationLineModel()).ToList()
    };

    public static AllocationLineModel ToAllocationLineModel(this AllocationLine line) => new()
    {
        Id = line.Id!,
        OrderItemId = line.OrderItemId,
        ProductId = line.ProductId,
        QuantityRequested = line.QuantityRequested,
        Status = (ContractAllocationLineStatus)line.Status
    };

    public static OrderAllocationConsumptionIntegrationEvent ToOrderAllocationConsumptionIntegrationEvent(
        this Allocation allocation,
        IReadOnlyDictionary<string, Batch> batchesById) => new()
    {
        OrderId = allocation.OrderId,
        AllocationId = allocation.Id!,
        Lines = allocation.Lines
            .Select(line => line.ToOrderAllocationConsumptionLineModel(batchesById))
            .ToList()
    };

    public static OrderAllocationConsumptionLineModel ToOrderAllocationConsumptionLineModel(
        this AllocationLine line,
        IReadOnlyDictionary<string, Batch> batchesById) => new()
    {
        OrderItemId = line.OrderItemId,
        ProductId = line.ProductId,
        QuantityRequested = line.QuantityRequested,
        BatchAllocations = line.BatchAllocations
            .Select(batchAllocation => batchAllocation.ToOrderAllocationConsumptionBatchModel(batchesById))
            .ToList()
    };

    public static OrderAllocationConsumptionBatchModel ToOrderAllocationConsumptionBatchModel(
        this BatchAllocation batchAllocation,
        IReadOnlyDictionary<string, Batch> batchesById) => new()
    {
        BatchId = batchAllocation.BatchId,
        Quantity = batchAllocation.QuantityAllocated,
        UnitPrice = batchesById.TryGetValue(batchAllocation.BatchId, out var batch)
            ? batch.PurchasePrice
            : 0m
    };
}
