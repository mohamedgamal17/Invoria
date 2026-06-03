using Invoria.Ordering.Contracts.Orders.Models;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Orders.Extensions;

public static class OrderMappingExtensions
{
    public static OrderModel ToOrderModel(this Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        CustomerId = order.CustomerId,
        OrderStatus = order.Status,
        PaymentType = order.PaymentType,
        PaymentStatus = order.PaymentStatus,
        TotalOrderAmount = order.TotalOrderAmount,
        AmountPaid = order.AmountPaid,
        AmountOutstanding = order.AmountOutstanding,
        Lines = order.Items.Select(i => i.ToOrderLineModel()).ToList()
    };

    public static OrderLineModel ToOrderLineModel(this OrderItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        Quantity = item.Quantity,
        UnitPrice = item.Price,
        LineTotal = item.Price * item.Quantity
    };
}
