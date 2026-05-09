using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Ordering.Domain.Orders;

public class OrderPayment : AuditedEntity
{
    public string OrderId { get; private set; } = null!;
    public decimal PaidAmount { get; private set; }
    public OrderPaymentMethod PaymentMethod { get; private set; }
    public DateTimeOffset PaidAt { get; private set; }

    public Order? Order { get; private set; }

    private OrderPayment()
    {
    }

    public OrderPayment(string orderId, decimal paidAmount, OrderPaymentMethod paymentMethod, DateTimeOffset paidAt)
    {
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, OrderPaymentTableConsts.OrderIdMaxLength);
        Guard.Against.NegativeOrZero(paidAmount);

        OrderId = orderId;
        PaidAmount = paidAmount;
        PaymentMethod = paymentMethod;
        PaidAt = paidAt;
    }
}
