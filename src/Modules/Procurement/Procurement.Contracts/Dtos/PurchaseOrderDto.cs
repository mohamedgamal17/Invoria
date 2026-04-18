using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Procurement.Contracts.PurchaseOrders;

namespace Invoria.Procurement.Contracts.Dtos;

public sealed class PurchaseOrderDto : AuditedEntityDto
{
    public string PurchaseNumber { get; set; } = default!;
    public string SupplierId { get; set; } = default!;
    public PurchaseOrderSupplierSummaryDto? Supplier { get; set; }
    public PurchaseState State { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseOrderItemDto> PurchaseOrderItems { get; set; } = [];
}
