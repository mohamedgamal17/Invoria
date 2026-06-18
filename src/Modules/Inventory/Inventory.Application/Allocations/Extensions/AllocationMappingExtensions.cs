using ContractAllocationLineStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationLineStatus;
using ContractAllocationStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationStatus;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Inventory.Domain.Allocations;

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
}
