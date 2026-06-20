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
public class OrderRevisionRequestedDomainEventHandlerTests
{
    private static void AssignStringEntityId(Entity<string> entity, string id)
    {
        var prop = typeof(Entity<string>).GetProperty(
            nameof(Entity<string>.Id),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        prop.SetValue(entity, id);
    }

    [Test]
    public async Task Publishes_OrderRevisionRequestedIntegrationEvent()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderRevisionRequestedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderRevisionRequestedDomainEventHandler>>());

        var order = new Order("ON-1", "c1", OrderPaymentType.Debt);
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 50m) });
        AssignStringEntityId(order, "o1");
        AssignStringEntityId(order.Items[0], "i1");
        order.Accept();
        order.RecordAllocation("alloc-1");
        order.MarkAsAllocated();
        order.RequestRevision();
        order.ClearDomainEvents();

        var ev = new OrderRevisionRequestedDomainEvent(order);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderRevisionRequestedIntegrationEvent>(e =>
                    e.Order.Id == "o1" &&
                    e.Order.OrderNumber == "ON-1" &&
                    e.Order.CustomerId == "c1" &&
                    e.Order.OrderStatus == OrderStatus.RevisionPending &&
                    e.AllocationId == "alloc-1" &&
                    e.OccurredOn == ev.OccurredOn),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
