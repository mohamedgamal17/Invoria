using FluentAssertions;
using Invoria.Financial.Contracts.Receivables.Events;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class CreateOrderReceivableSagaActivityHandlerTests
{
    [Test]
    public async Task Publishes_CreateOrderReceivableIntegrationEvent_from_activity()
    {
        var bus = new Mock<IBus>();
        var publishSetup = bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()));
        publishSetup.Returns(Task.CompletedTask);

        var handler = new CreateOrderReceivableSagaActivityHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CreateOrderReceivableSagaActivityHandler>>());

        await handler.Handle(new CreateOrderReceivableSagaActivity("order-1", "cust-1", 100m));

        bus.Verify(
            b => b.Publish(
                It.Is<CreateOrderReceivableIntegrationEvent>(e =>
                    e.OrderId == "order-1" &&
                    e.CustomerId == "cust-1" &&
                    e.Amount == 100m),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
