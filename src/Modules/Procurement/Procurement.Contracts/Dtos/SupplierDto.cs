using Invoria.BuildingBlocks.Domain.Dtos;

namespace Invoria.Procurement.Contracts.Dtos;

public sealed class SupplierDto : AuditedEntityDto
{
    public string SupplierCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }
}

