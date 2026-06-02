namespace Invoria.Ordering.Contracts.Orders.Models
{
    public class OrderItemModel
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public int  Quantity { get; set; }
    }
}
