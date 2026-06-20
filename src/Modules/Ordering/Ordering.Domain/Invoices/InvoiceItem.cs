using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.Invoices
{
    public class InvoiceItem : Entity
    {
        public string OrderItemId { get; private set; }
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        private InvoiceItem()
        {
        }

        public InvoiceItem(string orderItemId, string productId, int quantity, decimal price)
        {
            OrderItemId = orderItemId;
            ProductId = productId;
            Quantity = quantity;
            Price = price;
        }
    }
}
