using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.CustomerManagement.Contracts.Dtos;

namespace Invoria.Ordering.Contracts.Dtos
{
    public class OrderDto : AuditedEntityDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public CustomerDto? Customer { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
