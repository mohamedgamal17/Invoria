using Invoria.BuildingBlocks.Domain.Dtos;

namespace Invoria.Inventory.Contracts.Batches.Dtos;

public class BatchDto : AuditedEntityDto
{
    public string ProductId { get; set; } = string.Empty;
    public string? PurchaseOrderItemId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public decimal PurchasePrice { get; set; }
}
