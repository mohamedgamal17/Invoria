namespace Invoria.Procurement.Contracts.Dtos;

public sealed class PurchaseOrderItemDto
{
    public string Id { get; set; } = default!;
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? SupplierProductCode { get; set; }
    public decimal LineTotal { get; set; }
}
