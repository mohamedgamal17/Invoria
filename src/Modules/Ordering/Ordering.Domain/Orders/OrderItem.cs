using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.Orders
{
    public class OrderItem : AuditedEntity
    {   
        public string ProductId { get; private set; }
        public int Quantity { get;private set; }
        public decimal Price { get;private set; }

        private OrderItem()
        {
            
        }
        public OrderItem(string productId, int quantity, decimal price)
        {
            ProductId = productId;
            Quantity = quantity;
            Price = price;
        }
    }
}
