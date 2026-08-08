using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Contracts.Orders.Dtos;

public class OrderStateTransitionHistoryDto
{
    public string Id { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;

    public OrderStatus FromStatus { get; set; }

    public OrderStatus ToStatus { get; set; }

    public DateTimeOffset ChangedAt { get; set; }
}