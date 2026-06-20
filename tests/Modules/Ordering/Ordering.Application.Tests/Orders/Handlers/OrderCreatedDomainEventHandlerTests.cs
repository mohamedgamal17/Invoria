using Invoria.Ordering.Application.Orders.Handlers;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Orders.Handlers;

[TestFixture]
public class OrderCreatedDomainEventHandlerTests : OrderTestFixture
{
    [SetUp]
    public void ResetBusMock()
    {
        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Reset();
        busMock
            .Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);
    }

    [Test]
    public async Task Publishes_OrderCreatedIntegrationEvent()
    {
        var bus = ServiceProvider.GetRequiredService<Mock<IBus>>();
        var handler = new OrderCreatedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderCreatedDomainEventHandler>>());

        var order = Order.Create(
            "ON-1",
            "c1",
            OrderPaymentType.Debt,
            [new OrderItem("p1", 2, 50m)]);

        var lineId = order.Items[0].Id;

        var ev = new OrderCreatedDomainEvent(order);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderCreatedIntegrationEvent>(e =>
                    e.Order.Id == order.Id &&
                    e.Order.OrderNumber == "ON-1" &&
                    e.Order.CustomerId == "c1" &&
                    e.Order.OrderStatus == OrderStatus.Pending &&
                    e.Order.PaymentType == OrderPaymentType.Debt &&
                    e.Order.PaymentStatus == OrderPaymentStatus.Unpaid &&
                    e.Order.TotalOrderAmount == 100m &&
                    e.Order.AmountPaid == 0m &&
                    e.Order.AmountOutstanding == 100m &&
                    e.OccurredOn == ev.OccurredOn &&
                    e.Order.Lines.Count == 1 &&
                    e.Order.Lines[0].Id == lineId &&
                    e.Order.Lines[0].ProductId == "p1" &&
                    e.Order.Lines[0].Quantity == 2 &&
                    e.Order.Lines[0].UnitPrice == 50m &&
                    e.Order.Lines[0].LineTotal == 100m),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
