using FluentAssertions;
using Invoria.Inventory.Contracts.Allocations.Enums;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
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
    public void Deliver_duplicate_OrderCreatedIntegrationEvent_overwrites_existing_saga()
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

        data.OrderNumber.Should().Be("ON-DUPLICATE");
        data.CustomerId.Should().Be("cust-duplicate");
        data.State.Should().Be(OrderSagaProcessState.Created);
    }

    [Test]
    public void Deliver_OrderAccepted_after_created_sets_request_allocation_and_publishes_allocate_event()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.RequestAllocation);

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
    public void Deliver_duplicate_OrderAccepted_publishes_allocate_event_on_each_delivery()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));

        fixture.HandlerExceptions.Should().BeEmpty();

        bus.Verify(
            b => b.Publish(It.IsAny<AllocateOrderIntegrationEvent>(), It.IsAny<Dictionary<string, string>>()),
            Times.Exactly(2));
    }

    [Test]
    public void Deliver_AllocationCreated_after_request_allocation_sets_allocate_and_publishes_saga_activity()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationCreated("order-1", "alloc-1", "line-1", "p1", 2));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.Allocate);
        data.AllocationId.Should().Be("alloc-1");

        bus.Verify(
            b => b.Publish(
                It.Is<RecordOrderAllocationSagaActivity>(a =>
                    a.OrderId == "order-1" &&
                    a.AllocationId == "alloc-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_AllocationCreated_without_saga_does_not_publish_saga_activity()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildAllocationCreated("order-orphan", "alloc-1", "line-1", "p1", 1));

        fixture.HandlerExceptions.Should().BeEmpty();

        bus.Verify(
            b => b.Publish(It.IsAny<RecordOrderAllocationSagaActivity>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Test]
    public void Deliver_duplicate_AllocationCreated_publishes_saga_activity_on_each_delivery()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationCreated("order-1", "alloc-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationCreated("order-1", "alloc-1", "line-1", "p1", 2));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.Allocate);

        bus.Verify(
            b => b.Publish(It.IsAny<RecordOrderAllocationSagaActivity>(), It.IsAny<Dictionary<string, string>>()),
            Times.Exactly(2));
    }

    [Test]
    public void Deliver_AllocationFailed_after_request_allocation_publishes_revise_saga_activity()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationFailed("order-1", "alloc-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.AllocationFailed);
        data.AllocationId.Should().Be("alloc-1");

        bus.Verify(
            b => b.Publish(
                It.Is<ReviseOrderSagaActivity>(a => a.OrderId == "order-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_AllocationFailed_without_saga_does_not_publish_revise_saga_activity()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildAllocationFailed("order-orphan", "alloc-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        bus.Verify(
            b => b.Publish(It.IsAny<ReviseOrderSagaActivity>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Test]
    public void Deliver_duplicate_AllocationFailed_publishes_revise_saga_activity_on_each_delivery()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationFailed("order-1", "alloc-1"));
        fixture.Deliver(BuildAllocationFailed("order-1", "alloc-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.AllocationFailed);
        data.AllocationId.Should().Be("alloc-1");

        bus.Verify(
            b => b.Publish(It.IsAny<ReviseOrderSagaActivity>(), It.IsAny<Dictionary<string, string>>()),
            Times.Exactly(2));
    }

    [Test]
    public void Deliver_AllocationSucceeded_after_allocate_publishes_mark_order_allocated_saga_activity()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationCreated("order-1", "alloc-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationSucceeded("order-1", "alloc-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.AllocationSucceeded);
        data.AllocationId.Should().Be("alloc-1");

        bus.Verify(
            b => b.Publish(
                It.Is<MarkOrderAllocatedSagaActivity>(a => a.OrderId == "order-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_AllocationSucceeded_without_saga_does_not_publish_mark_order_allocated_saga_activity()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildAllocationSucceeded("order-orphan", "alloc-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        bus.Verify(
            b => b.Publish(It.IsAny<MarkOrderAllocatedSagaActivity>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Test]
    public void Deliver_duplicate_AllocationSucceeded_publishes_mark_order_allocated_saga_activity_on_each_delivery()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationCreated("order-1", "alloc-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationSucceeded("order-1", "alloc-1"));
        fixture.Deliver(BuildAllocationSucceeded("order-1", "alloc-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.AllocationSucceeded);
        data.AllocationId.Should().Be("alloc-1");

        bus.Verify(
            b => b.Publish(It.IsAny<MarkOrderAllocatedSagaActivity>(), It.IsAny<Dictionary<string, string>>()),
            Times.Exactly(2));
    }

    [Test]
    public void Deliver_OrderAccepted_when_state_is_allocate_publishes_allocate_event_and_sets_request_allocation()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Add(new OrderSagaState
        {
            OrderId = "order-1",
            OrderNumber = "ON-1",
            CustomerId = "cust-1",
            AllocationId = "alloc-1",
            State = OrderSagaProcessState.Allocate
        });

        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.RequestAllocation);
        data.AllocationId.Should().Be("alloc-1");

        bus.Verify(
            b => b.Publish(It.IsAny<AllocateOrderIntegrationEvent>(), It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_OrderRevisionRequested_after_allocation_succeeded_publishes_release_allocation_event()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderCreated("order-1", "ON-1", "cust-1"));
        fixture.Deliver(BuildOrderAccepted("order-1", "ON-1", "cust-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationCreated("order-1", "alloc-1", "line-1", "p1", 2));
        fixture.Deliver(BuildAllocationSucceeded("order-1", "alloc-1"));
        fixture.Deliver(BuildOrderRevisionRequested("order-1", "ON-1", "cust-1", "alloc-1", "line-1", "p1", 2));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderSagaProcessState.RevisionRequested);
        data.AllocationId.Should().Be("alloc-1");

        bus.Verify(
            b => b.Publish(
                It.Is<ReleaseAllocationIntegrationEvent>(e => e.AllocationId == "alloc-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_OrderRevisionRequested_without_saga_does_not_publish_release_allocation_event()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderSaga(bus.Object));

        fixture.Deliver(BuildOrderRevisionRequested(
            "order-orphan",
            "ON-X",
            "cust-x",
            "alloc-1",
            "line-1",
            "p1",
            1));

        fixture.HandlerExceptions.Should().BeEmpty();

        bus.Verify(
            b => b.Publish(It.IsAny<ReleaseAllocationIntegrationEvent>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
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

    private static AllocationCreatedIntegrationEvent BuildAllocationCreated(
        string orderId,
        string allocationId,
        string lineId,
        string productId,
        int quantity) =>
        new()
        {
            OccurredOn = DateTimeOffset.UtcNow,
            Allocation = new AllocationModel
            {
                Id = allocationId,
                OrderId = orderId,
                Status = AllocationStatus.Pending,
                Lines =
                [
                    new AllocationLineModel
                    {
                        Id = "al-1",
                        OrderItemId = lineId,
                        ProductId = productId,
                        QuantityRequested = quantity,
                        Status = AllocationLineStatus.Pending
                    }
                ]
            }
        };

    private static AllocationFailedIntegrationEvent BuildAllocationFailed(
        string orderId,
        string allocationId) =>
        new()
        {
            OrderId = orderId,
            AllocationId = allocationId
        };

    private static AllocationSucceededIntegrationEvent BuildAllocationSucceeded(
        string orderId,
        string allocationId) =>
        new()
        {
            OrderId = orderId,
            AllocationId = allocationId
        };

    private static OrderRevisionRequestedIntegrationEvent BuildOrderRevisionRequested(
        string orderId,
        string orderNumber,
        string customerId,
        string allocationId,
        string lineId,
        string productId,
        int quantity) =>
        new()
        {
            OccurredOn = DateTimeOffset.UtcNow,
            AllocationId = allocationId,
            Order = new OrderModel
            {
                Id = orderId,
                OrderNumber = orderNumber,
                CustomerId = customerId,
                OrderStatus = OrderStatus.RevisionPending,
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
