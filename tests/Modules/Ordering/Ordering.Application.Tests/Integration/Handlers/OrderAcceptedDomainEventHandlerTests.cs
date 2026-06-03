using System.Reflection;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Application.Orders.Handlers;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Handlers;

[TestFixture]
public class OrderAcceptedDomainEventHandlerTests
{
    private static void AssignStringEntityId(Entity<string> entity, string id)
    {
        var prop = typeof(Entity<string>).GetProperty(
            nameof(Entity<string>.Id),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        prop.SetValue(entity, id);
    }

    [Test]
    public async Task Publishes_OrderAcceptedIntegrationEvent()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderAcceptedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderAcceptedDomainEventHandler>>());

        var order = new Order("ON-1", "c1", OrderPaymentType.Debt);
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 50m) });
        AssignStringEntityId(order, "o1");
        AssignStringEntityId(order.Items[0], "i1");
        order.Accept();
        order.ClearDomainEvents();

        var ev = new OrderAcceptedDomainEvent(order);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderAcceptedIntegrationEvent>(e =>
                    e.Order.Id == "o1" &&
                    e.Order.OrderNumber == "ON-1" &&
                    e.Order.CustomerId == "c1" &&
                    e.Order.OrderStatus == OrderStatus.Processing &&
                    e.Order.PaymentType == OrderPaymentType.Debt &&
                    e.Order.PaymentStatus == OrderPaymentStatus.Unpaid &&
                    e.Order.TotalOrderAmount == 100m &&
                    e.Order.AmountPaid == 0m &&
                    e.Order.AmountOutstanding == 100m &&
                    e.OccurredOn == ev.OccurredOn &&
                    e.Order.Lines.Count == 1 &&
                    e.Order.Lines[0].Id == "i1" &&
                    e.Order.Lines[0].ProductId == "p1" &&
                    e.Order.Lines[0].Quantity == 2 &&
                    e.Order.Lines[0].UnitPrice == 50m &&
                    e.Order.Lines[0].LineTotal == 100m),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
