using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Contracts.Orders.Models;

/// <summary>
/// Shared order snapshot for integration messaging (e.g. created or updated order notifications).
/// <see cref="Id"/> is the order aggregate id.
/// </summary>
public class OrderModel
{
    public required string Id { get; set; }

    public required string OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public OrderPaymentType PaymentType { get; set; }

    public OrderPaymentStatus PaymentStatus { get; set; }

    public decimal TotalOrderAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal AmountOutstanding { get; set; }

    public required List<OrderLineModel> Lines { get; set; }
}
