using Invoria.Catalog.Contracts.Dtos;
using ContractAllocationLineStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationLineStatus;

namespace Invoria.Inventory.Application.Allocations.Dtos;

public class AllocationLineDto
{
    public string Id { get; set; } = string.Empty;

    public string AllocationId { get; set; } = string.Empty;

    public string OrderItemId { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public int QuantityRequested { get; set; }

    public ContractAllocationLineStatus Status { get; set; }

    public ProductDto? Product { get; set; }
}
