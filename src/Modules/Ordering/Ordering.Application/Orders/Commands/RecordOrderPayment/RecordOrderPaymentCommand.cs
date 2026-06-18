using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderPayment;

public class RecordOrderPaymentCommand : ICommand<OrderDto>
{
    public string OrderId { get; set; }

    public decimal PaidAmount { get; set; }

    public OrderPaymentMethod PaymentMethod { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public RecordOrderPaymentCommand(
        string orderId,
        decimal paidAmount,
        OrderPaymentMethod paymentMethod,
        DateTimeOffset? paidAt = null)
    {
        OrderId = orderId;
        PaidAmount = paidAmount;
        PaymentMethod = paymentMethod;
        PaidAt = paidAt;
    }
}
