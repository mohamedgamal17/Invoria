namespace Invoria.Ordering.Contracts.Dtos
{
    public class OrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
