using Invoria.BuildingBlocks.Domain.Dtos;

namespace Invoria.Ordering.Contracts.Dtos
{
    public class OrderDto : AuditedEntityDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
