using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.Invoices
{
    public class InvoiceItem : Entity
    {
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        private InvoiceItem()
        {
        }

        public InvoiceItem(string productId, int quantity, decimal price)
        {
            ProductId = productId;
            Quantity = quantity;
            Price = price;
        }
    }
}
