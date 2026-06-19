using FluentAssertions;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Invoices.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class CreateOrderInvoiceSagaActivityHandlerTests
{
    [Test]
    public async Task Publishes_OrderInvoiceRequestIntegrationEvent_from_activity()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateOrderInvoiceSagaActivityHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CreateOrderInvoiceSagaActivityHandler>>());

        await handler.Handle(new CreateOrderInvoiceSagaActivity("order-1"));

        bus.Verify(
            b => b.Publish(
                It.Is<OrderInvoiceRequestIntegrationEvent>(e => e.OrderId == "order-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
