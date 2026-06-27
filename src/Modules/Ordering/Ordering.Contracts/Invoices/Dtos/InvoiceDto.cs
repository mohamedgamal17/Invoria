using Invoria.BuildingBlocks.Domain.Dtos;

namespace Invoria.Ordering.Contracts.Invoices.Dtos;

public class InvoiceDto : AuditedEntityDto
{
    public string? InvoiceNumber { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TotalPrice { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}
