using Invoria.BuildingBlocks.Domain.Dtos;

namespace Invoria.Procurement.Contracts.Dtos;

public sealed class PurchaseOrderSupplierSummaryDto : AuditedEntityDto
{
    public string SupplierCode { get; set; } = default!;
    public string Name { get; set; } = default!;
}
