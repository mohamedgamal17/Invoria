namespace Invoria.Ordering.Contracts.Invoices.Dtos;

public class InvoiceItemDto
{
    public string OrderItemId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal LineTotal => Price * Quantity;
}
