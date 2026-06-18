using Invoria.Catalog.Contracts.Dtos;

namespace Invoria.Ordering.Contracts.Orders.Dtos
{
    public class OrderItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public ProductDto? Product { get; set; }
    }
}
