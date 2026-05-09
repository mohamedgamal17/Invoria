using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Ordering.Contracts.Dtos;

public class OrderPaymentDto : AuditedEntityDto
{
    public string OrderId { get; set; } = string.Empty;

    public decimal PaidAmount { get; set; }

    public OrderPaymentMethod PaymentMethod { get; set; }

    public DateTimeOffset PaidAt { get; set; }
}
