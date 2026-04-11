using Invoria.Ordering.Application.Orders.Handlers;
using Microsoft.Extensions.Logging;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Domain.Orders.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Handlers;

[TestFixture]
public class OrderRefusedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_OrderRefusedIntegrationEvent()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderRefusedDomainEventHandler(bus.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderRefusedDomainEventHandler>>());
        var ev = new OrderRefusedDomainEvent("o1", "ON-1", "c1");

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderRefusedIntegrationEvent>(e =>
                    e.OrderId == "o1" &&
                    e.OrderNumber == "ON-1" &&
                    e.CustomerId == "c1" &&
                    e.RefusedAt != default),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
