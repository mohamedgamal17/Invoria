using Invoria.BuildingBlocks.Domain.Dtos;
using ContractAllocationStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationStatus;

namespace Invoria.Inventory.Application.Allocations.Dtos;

public class AllocationDto : AuditedEntityDto
{
    public string OrderId { get; set; } = string.Empty;

    public ContractAllocationStatus Status { get; set; }

    public List<AllocationLineDto> Lines { get; set; } = [];
}
