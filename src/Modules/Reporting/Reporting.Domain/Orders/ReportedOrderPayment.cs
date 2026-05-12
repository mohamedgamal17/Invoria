using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Reporting.Domain.Orders;

public class ReportedOrderPayment : IBaseEntity
{
    public string Id { get; set; } = null!;
    public string ReportedOrderId { get; set; } = null!;
    public decimal PaidAmount { get; set; }
    public OrderPaymentMethod PaymentMethod { get; set; }
    public DateTimeOffset PaidAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public ReportedOrder? ReportedOrder { get; set; }
}
