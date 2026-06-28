using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Contracts.Orders.Dtos
{
    public class OrderDto : AuditedEntityDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public CustomerDto? Customer { get; set; }
        public OrderStatus Status { get; set; }
        public OrderPaymentType PaymentType { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountOutstanding { get; set; }
        public OrderPaymentStatus PaymentStatus { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderReturnItemDto> ReturnItems { get; set; } = new();
        public decimal TotalOrderAmount { get; set; }
        public decimal NetOfTotalOrderAmount { get; set; }
        public decimal ReturnsTotal { get; set; }
        public List<OrderPaymentDto> Payments { get; set; } = new();
        public string? AllocationId { get; set; }
        public string? ReturnId { get; set; }
        public string? InvoiceId { get; set; }
        public bool OrderAllocated { get; set; }
    }
}
