using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Reporting.Contracts.Dtos;

public sealed class OrderStatusSummaryItemDto
{
    public OrderStatus Status { get; set; }

    public int Count { get; set; }
}
