using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Reporting.Domain.Orders;

public class ReportedOrderStateTransition : IBaseEntity
{
    public string Id { get; set; } = null!;
    public string ReportedOrderId { get; set; } = null!;
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public FullfillmentStatus FromFullfillmentStatus { get; set; }
    public FullfillmentStatus ToFullfillmentStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? Reason { get; set; }

    public ReportedOrder? ReportedOrder { get; set; }
}
