using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Ordering.Contracts.Dtos
{
    public class OrderDto : AuditedEntityDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public CustomerDto? Customer { get; set; }
        public OrderStatus Status { get; set; }
        public FullfillmentStatus FullfillmentStatus { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderFailureDetailsDto> FailureDetails { get; set; } = new();
        public List<OrderStateTransitionHistoryDto> StateTransitionHistory { get; set; } = new();
    }
}
