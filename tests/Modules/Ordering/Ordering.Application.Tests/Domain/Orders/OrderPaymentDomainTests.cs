using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderPaymentDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    private static Order CreateOrderWithItems(
        OrderPaymentType paymentType,
        params OrderItem[] items)
    {
        var order = new Order("PAY-1", Guid.NewGuid().ToString(), paymentType);
        SetEntityId(order, "order-pay-1");
        order.UpdateItems(items.ToList());
        return order;
    }

    /// <summary>
    /// Brings order to Completed so payments are allowed (<see cref="Order.RecordPayment" /> gate).
    /// </summary>
    private static void CompleteViaFulfillment(Order order)
    {
        order.Accept();
        order.Complete([]);
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public void TotalOrderAmount_is_sum_of_line_extensions()
    {
        var order = CreateOrderWithItems(
            OrderPaymentType.Debt,
            new OrderItem("a", 2, 10m),
            new OrderItem("b", 1, 5m));

        order.TotalOrderAmount.Should().Be(25m);
    }

    [Test]
    public void TotalOrderAmount_with_no_items_is_zero()
    {
        var order = new Order("PAY-EMPTY", Guid.NewGuid().ToString());
        SetEntityId(order, "order-empty");

        order.TotalOrderAmount.Should().Be(0m);
    }

    [Test]
    public void PaymentStatus_Unpaid_when_no_payments()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 1, 100m));

        order.AmountPaid.Should().Be(0m);
        order.AmountOutstanding.Should().Be(100m);
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Unpaid);
    }

    [Test]
    public void RecordPayment_throws_when_order_not_completed()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 1, 100m));

        order.Invoking(o => o.RecordPayment(10m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Test]
    public void PaymentStatus_Partial_after_partial_debt_payment()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 1, 100m));
        CompleteViaFulfillment(order);
        var paidAt = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        order.RecordPayment(40m, OrderPaymentMethod.Cash, paidAt);

        order.AmountPaid.Should().Be(40m);
        order.AmountOutstanding.Should().Be(60m);
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Partial);
        order.Payments.Should().ContainSingle();
        order.Payments[0].PaidAmount.Should().Be(40m);
        order.Payments[0].PaymentMethod.Should().Be(OrderPaymentMethod.Cash);
        order.Payments[0].PaidAt.Should().Be(paidAt);
        order.Payments[0].OrderId.Should().Be("order-pay-1");
    }

    [Test]
    public void PaymentStatus_Paid_when_fully_paid_debt()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 2, 25m));
        CompleteViaFulfillment(order);

        order.RecordPayment(30m, OrderPaymentMethod.BankTransfer, DateTimeOffset.UtcNow);
        order.RecordPayment(20m, OrderPaymentMethod.Cheque, DateTimeOffset.UtcNow);

        order.AmountPaid.Should().Be(50m);
        order.AmountOutstanding.Should().Be(0m);
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Paid);
    }

    [Test]
    public void Immediate_RecordPayment_succeeds_when_amount_equals_total()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Immediate, new OrderItem("p", 1, 99.5m));
        CompleteViaFulfillment(order);

        order.Invoking(o => o.RecordPayment(99.5m, OrderPaymentMethod.Other, DateTimeOffset.UtcNow))
            .Should().NotThrow();

        order.PaymentStatus.Should().Be(OrderPaymentStatus.Paid);
        order.Payments.Should().ContainSingle();
    }

    [Test]
    public void Immediate_RecordPayment_fails_when_amount_not_equal_total()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Immediate, new OrderItem("p", 1, 100m));
        CompleteViaFulfillment(order);

        order.Invoking(o => o.RecordPayment(99m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Immediate_second_RecordPayment_fails()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Immediate, new OrderItem("p", 1, 10m));
        CompleteViaFulfillment(order);
        order.RecordPayment(10m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow);

        order.Invoking(o => o.RecordPayment(10m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Debt_RecordPayment_fails_when_exceeds_outstanding()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 1, 100m));
        CompleteViaFulfillment(order);
        order.RecordPayment(60m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow);

        order.Invoking(o => o.RecordPayment(50m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RecordPayment_fails_when_amount_not_positive()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 1, 100m));
        CompleteViaFulfillment(order);

        order.Invoking(o => o.RecordPayment(0m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RecordPayment_fails_when_fully_paid()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 1, 50m));
        CompleteViaFulfillment(order);
        order.RecordPayment(50m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow);

        order.Invoking(o => o.RecordPayment(1m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RecordPayment_fails_when_no_items()
    {
        var order = new Order("PAY-NO-ITEMS", Guid.NewGuid().ToString(), OrderPaymentType.Debt);
        SetEntityId(order, "order-no-items");

        order.Invoking(o => o.RecordPayment(10m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RecordPayment_fails_when_total_is_zero()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 1, 0m));
        CompleteViaFulfillment(order);

        order.Invoking(o => o.RecordPayment(0.01m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Order_default_payment_type_is_Immediate()
    {
        var order = new Order("DEF", "cust");

        order.PaymentType.Should().Be(OrderPaymentType.Immediate);
        order.Payments.Should().BeEmpty();
    }

    [Test]
    public void After_RecordPayment_persisted_summary_matches_net_minus_paid()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 2, 15m));
        CompleteViaFulfillment(order);
        order.RecordPayment(10m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow);

        order.AmountOutstanding.Should().Be(order.NetOfTotalOrderAmount - order.AmountPaid);
        order.AmountPaid.Should().Be(10m);
        order.TotalOrderAmount.Should().Be(30m);
    }

    [Test]
    public void Immediate_RecordPayment_succeeds_when_amount_equals_net_after_returns()
    {
        var order = CreateOrderWithItems(
            OrderPaymentType.Immediate,
            new OrderItem("p", 2, 50m));
        SetEntityId(order.Items[0], "line-1");
        order.Accept();
        order.Complete([new OrderReturnItem("line-1", 1)]);
        order.NetOfTotalOrderAmount.Should().Be(50m);

        order.Invoking(o => o.RecordPayment(100m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().Throw<InvalidOperationException>();

        order.Invoking(o => o.RecordPayment(50m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow))
            .Should().NotThrow();

        order.PaymentStatus.Should().Be(OrderPaymentStatus.Paid);
    }

    [Test]
    public void UpdateItems_while_pending_recomputes_outstanding_when_no_payments_yet()
    {
        var order = CreateOrderWithItems(OrderPaymentType.Debt, new OrderItem("p", 1, 100m));

        order.UpdateItems([new OrderItem("p", 1, 50m)]);

        order.TotalOrderAmount.Should().Be(50m);
        order.AmountPaid.Should().Be(0m);
        order.AmountOutstanding.Should().Be(50m);
        order.PaymentStatus.Should().Be(OrderPaymentStatus.Unpaid);
    }
}
