using FluentAssertions;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using Rebus.TestHelpers;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class OrderSagaTests
{
    [Test]
    public void Deliver_OrderCreatedIntegrationEvent_creates_saga_with_created_state()
    {
        using var fixture = SagaFixture.For(() => new OrderSaga());

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.OrderNumber.Should().Be("ON-1");
        data.CustomerId.Should().Be("cust-1");
        data.State.Should().Be(OrderSagaProcessState.Created);
    }

    [Test]
    public void Deliver_duplicate_OrderCreatedIntegrationEvent_does_not_overwrite_existing_saga()
    {
        using var fixture = SagaFixture.For(() => new OrderSaga());

        fixture.Add(new OrderSagaState
        {
            OrderId = "order-1",
            OrderNumber = "ON-ORIGINAL",
            CustomerId = "cust-original",
            State = OrderSagaProcessState.Created
        });

        fixture.Deliver(BuildOrderCreated("order-1", "ON-DUPLICATE", "cust-duplicate"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.OrderNumber.Should().Be("ON-ORIGINAL");
        data.CustomerId.Should().Be("cust-original");
    }

    private static OrderCreatedIntegrationEvent BuildOrderCreated(
        string orderId,
        string orderNumber,
        string customerId) =>
        new()
        {
            OccurredOn = DateTimeOffset.UtcNow,
            Order = new OrderModel
            {
                Id = orderId,
                OrderNumber = orderNumber,
                CustomerId = customerId,
                OrderStatus = OrderStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 100m,
                AmountPaid = 0m,
                AmountOutstanding = 100m,
                Lines = []
            }
        };
}
