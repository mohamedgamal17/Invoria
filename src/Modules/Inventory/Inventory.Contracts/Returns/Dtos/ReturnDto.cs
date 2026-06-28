using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Inventory.Contracts.Returns.Enums;

namespace Invoria.Inventory.Contracts.Returns.Dtos;

public class ReturnDto : AuditedEntityDto
{
    public ReturnType Type { get; set; }

    public ReturnStatus Status { get; set; }

    public List<ReturnLineDto> ReturnLines { get; set; } = [];

    public string? AllocationId { get; set; }

    public string? OrderId { get; set; }
}
