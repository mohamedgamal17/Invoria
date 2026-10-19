using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Contracts.Orders.Dtos
{
    public class PublicOrderDto
    {
        public string OrderNumber { get; set; } 
        public string CustomerId { get; set; }
        public OrderStatus Status { get; set; }
        public OrderPaymentType PaymentType { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal ReturnsAmount { get; set; }
        public decimal NetOrderAmount { get; set; }
        public decimal AmountOutstanding { get; set; }
        public string? AllocationId { get; set; }
        public string? ReturnId { get; set; }
        public string? InvoiceId { get; set; }
        public bool OrderAllocated { get; set; }
    }
}
