using FluentAssertions;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using Moq;
using Rebus.Bus;
using Rebus.TestHelpers;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class OrderReturnSagaTests
{
    [Test]
    public void Deliver_OrderReturnRequestedIntegrationEvent_creates_saga_with_requested_state()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderReturnSaga(bus.Object));

        fixture.Deliver(BuildOrderReturnRequested("order-1", "alloc-1", "line-1", "p1", 1));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderReturnSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderReturnSagaProcessState.Requested);
        data.ReturnId.Should().BeNull();

        bus.Verify(
            b => b.Publish(
                It.Is<CreateImmediateReturnIntegrationEvent>(e =>
                    e.OrderId == "order-1" &&
                    e.AllocationId == "alloc-1" &&
                    e.Lines.Count == 1 &&
                    e.Lines[0].OrderItemId == "line-1" &&
                    e.Lines[0].ProductId == "p1" &&
                    e.Lines[0].Quantity == 1),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_ImmediateReturnCreatedIntegrationEvent_sets_completed_state_and_return_id()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderReturnSaga(bus.Object));

        fixture.Deliver(BuildOrderReturnRequested("order-1", "alloc-1", "line-1", "p1", 1));
        fixture.Deliver(BuildImmediateReturnCreated("return-1", "order-1", "alloc-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderReturnSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderReturnSagaProcessState.Completed);
        data.ReturnId.Should().Be("return-1");

        bus.Verify(
            b => b.Publish(
                It.Is<RecordOrderReturnSagaActivity>(a =>
                    a.OrderId == "order-1" &&
                    a.ReturnId == "return-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_ImmediateReturnCreated_without_saga_does_not_fail()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderReturnSaga(bus.Object));

        fixture.Deliver(BuildImmediateReturnCreated("return-orphan", "order-orphan", "alloc-orphan"));

        fixture.HandlerExceptions.Should().BeEmpty();
        fixture.Data.OfType<OrderReturnSagaState>().Should().BeEmpty();
    }

    private static Mock<IBus> CreateBus()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);
        return bus;
    }

    private static OrderReturnRequestedIntegrationEvent BuildOrderReturnRequested(
        string orderId,
        string allocationId,
        string lineId,
        string productId,
        int quantity) =>
        new()
        {
            OrderId = orderId,
            AllocationId = allocationId,
            Lines =
            [
                new OrderReturnLineModel
                {
                    OrderItemId = lineId,
                    ProductId = productId,
                    Quantity = quantity
                }
            ]
        };

    private static ImmediateReturnCreatedIntegrationEvent BuildImmediateReturnCreated(
        string returnId,
        string orderId,
        string allocationId) =>
        new()
        {
            ReturnId = returnId,
            OrderId = orderId,
            AllocationId = allocationId
        };
}
