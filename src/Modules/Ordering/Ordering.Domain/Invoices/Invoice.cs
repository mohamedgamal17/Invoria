using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.Invoices
{
    public class Invoice : AuditedAggregateRoot
    {
        public string? InvoiceNumber { get; private set; }
        public string CustomerId { get; private set; }
        public string OrderId { get; private set; }
        public decimal Subtotal { get; private set; }
        public decimal TotalPrice { get; private set; }
        public List<InvoiceItem> Items { get; private set; }

        private Invoice()
        {
            Items = new List<InvoiceItem>();
        }

        private Invoice(
            string id,
            string invoiceNumber,
            string customerId,
            string orderId,
            decimal subtotal,
            decimal totalPrice)
        {
            Id = id;
            InvoiceNumber = invoiceNumber;
            CustomerId = customerId;
            OrderId = orderId;
            Subtotal = subtotal;
            TotalPrice = totalPrice;
            Items = new List<InvoiceItem>();
        }

        public static Invoice Create(
            string invoiceNumber,
            string customerId,
            string orderId,
            decimal subtotal,
            decimal totalPrice,
            List<InvoiceItem> items)
        {
            Guard.Against.NullOrWhiteSpace(customerId);
            Guard.Against.NullOrWhiteSpace(orderId);
            Guard.Against.Negative(subtotal);
            Guard.Against.Negative(totalPrice);
            Guard.Against.NullOrEmpty(items);

            var invoice = new Invoice(
                Guid.NewGuid().ToString("N"),
                invoiceNumber,
                customerId,
                orderId,
                subtotal,
                totalPrice);

            invoice.Items = items;

            return invoice;
        }
    }
}
