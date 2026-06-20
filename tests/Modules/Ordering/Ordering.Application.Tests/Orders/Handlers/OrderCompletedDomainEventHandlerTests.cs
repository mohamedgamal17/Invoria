using System.Reflection;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Application.Orders.Handlers;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Orders.Handlers;

[TestFixture]
public class OrderCompletedDomainEventHandlerTests
{
    private static void AssignStringEntityId(Entity<string> entity, string id)
    {
        var prop = typeof(Entity<string>).GetProperty(
            nameof(Entity<string>.Id),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        prop.SetValue(entity, id);
    }

    [Test]
    public async Task Publishes_OrderCompletedIntegrationEvent_with_return_lines_and_allocation()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderCompletedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderCompletedDomainEventHandler>>());

        var order = CreateProcessingOrder("o1", "ON-1", "c1", "i1", "p1", 2);
        order.RecordAllocation("alloc-1");
        order.Complete([new OrderReturnItem("i1", 1)]);
        order.ClearDomainEvents();

        var ev = new OrderCompletedDomainEvent(order);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderCompletedIntegrationEvent>(e =>
                    e.OrderId == "o1" &&
                    e.AllocationId == "alloc-1" &&
                    e.ReturnLines.Count == 1 &&
                    e.ReturnLines[0].OrderItemId == "i1" &&
                    e.ReturnLines[0].ProductId == "p1" &&
                    e.ReturnLines[0].Quantity == 1 &&
                    e.HasBillableItems),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Publishes_OrderCompletedIntegrationEvent_with_empty_return_lines_when_no_returns()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderCompletedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderCompletedDomainEventHandler>>());

        var order = CreateProcessingOrder("o1", "ON-1", "c1", "i1", "p1", 2);
        order.RecordAllocation("alloc-1");
        order.Complete([]);
        order.ClearDomainEvents();

        var ev = new OrderCompletedDomainEvent(order);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderCompletedIntegrationEvent>(e =>
                    e.OrderId == "o1" &&
                    e.AllocationId == "alloc-1" &&
                    e.ReturnLines.Count == 0 &&
                    e.HasBillableItems),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Publishes_OrderCompletedIntegrationEvent_with_HasBillableItems_when_no_returns()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderCompletedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderCompletedDomainEventHandler>>());

        var order = CreateProcessingOrder("o1", "ON-1", "c1", "i1", "p1", 2);
        order.Complete([]);
        order.ClearDomainEvents();

        var ev = new OrderCompletedDomainEvent(order);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderCompletedIntegrationEvent>(e =>
                    e.OrderId == "o1" &&
                    e.HasBillableItems),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Publishes_OrderCompletedIntegrationEvent_with_HasBillableItems_when_partial_returns()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderCompletedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderCompletedDomainEventHandler>>());

        var order = CreateProcessingOrder("o1", "ON-1", "c1", "i1", "p1", 2);
        order.RecordAllocation("alloc-1");
        order.Complete([new OrderReturnItem("i1", 1)]);
        order.ClearDomainEvents();

        var ev = new OrderCompletedDomainEvent(order);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderCompletedIntegrationEvent>(e =>
                    e.OrderId == "o1" &&
                    e.HasBillableItems),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Publishes_OrderCompletedIntegrationEvent_without_HasBillableItems_when_all_items_returned()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderCompletedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderCompletedDomainEventHandler>>());

        var order = CreateProcessingOrder("o1", "ON-1", "c1", "i1", "p1", 2);
        order.Complete([new OrderReturnItem("i1", 2)]);
        order.ClearDomainEvents();

        var ev = new OrderCompletedDomainEvent(order);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderCompletedIntegrationEvent>(e =>
                    e.OrderId == "o1" &&
                    !e.HasBillableItems),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    private static Order CreateProcessingOrder(
        string orderId,
        string orderNumber,
        string customerId,
        string lineId,
        string productId,
        int quantity)
    {
        var order = new Order(orderNumber, customerId, OrderPaymentType.Debt);
        order.UpdateItems([new OrderItem(productId, quantity, 50m)]);
        AssignStringEntityId(order, orderId);
        AssignStringEntityId(order.Items[0], lineId);
        order.Accept();
        order.ClearDomainEvents();
        return order;
    }
}
