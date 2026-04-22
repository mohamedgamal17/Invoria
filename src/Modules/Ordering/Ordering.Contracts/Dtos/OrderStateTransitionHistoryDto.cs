using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Ordering.Contracts.Dtos;

public class OrderStateTransitionHistoryDto
{
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public FullfillmentStatus FromFullfillmentStatus { get; set; }
    public FullfillmentStatus ToFullfillmentStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? Reason { get; set; }
}
