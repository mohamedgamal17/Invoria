using FluentAssertions;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using Moq;
using Rebus.Bus;
using Rebus.TestHelpers;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class OrderSagaTests
{
    [Test]
    public void Deliver_OrderCreatedIntegrationEvent_creates_saga_with_created_state()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.OrderNumber.Should().Be("ON-1");
        data.CustomerId.Should().Be("cust-1");
        data.State.Should().Be(OrderSagaProcessState.Created);

        bus.Verify(
            b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Test]
    public void Deliver_duplicate_OrderCreatedIntegrationEvent_does_not_overwrite_existing_saga()
    {
        using var fixture = SagaFixture.For(() => new OrderSaga(CreateBus().Object));

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

    [Test]
    public void Deliver_OrderAccepted_after_created_sets_allocating_and_publishes_allocate_event()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.Allocating);

        bus.Verify(
            b => b.Publish(
                It.Is<AllocateOrderIntegrationEvent>(e =>
                    e.Id == "order-1" &&
                    e.OrderNumber == "ON-1" &&
                    e.CustomerId == "cust-1" &&
                    e.Items.Count == 1 &&
                    e.Items[0].Id == "line-1" &&
                    e.Items[0].ProductId == "p1" &&
                    e.Items[0].Quantity == 2),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_OrderAccepted_without_saga_does_not_publish_allocate_event()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderAccepted("order-orphan", "ON-X", "cust-x", "line-1", "p1", 1));

        fixture.HandlerExceptions.Should().BeEmpty();

        bus.Verify(
            b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Test]
    public void Deliver_duplicate_OrderAccepted_does_not_publish_allocate_event_again()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));

        fixture.HandlerExceptions.Should().BeEmpty();

        bus.Verify(
            b => b.Publish(It.IsAny<AllocateOrderIntegrationEvent>(), It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    private static Mock<IBus> CreateBus()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);
        return bus;
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

    private static OrderAcceptedIntegrationEvent BuildOrderAccepted(
        string orderId,
        string orderNumber,
        string customerId,
        string lineId,
        string productId,
        int quantity) =>
        new()
        {
            OccurredOn = DateTimeOffset.UtcNow,
            Order = new OrderModel
            {
                Id = orderId,
                OrderNumber = orderNumber,
                CustomerId = customerId,
                OrderStatus = OrderStatus.Processing,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 100m,
                AmountPaid = 0m,
                AmountOutstanding = 100m,
                Lines =
                [
                    new OrderLineModel
                    {
                        Id = lineId,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = 50m,
                        LineTotal = 50m * quantity
                    }
                ]
            }
        };
}
