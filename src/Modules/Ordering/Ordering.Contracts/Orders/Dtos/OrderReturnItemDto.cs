using Invoria.Catalog.Contracts.Dtos;

namespace Invoria.Ordering.Contracts.Orders.Dtos;

public class OrderReturnItemDto
{
    public string OrderItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int OrderedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineReturnTotal { get; set; }
    public ProductDto? Product { get; set; }
}
