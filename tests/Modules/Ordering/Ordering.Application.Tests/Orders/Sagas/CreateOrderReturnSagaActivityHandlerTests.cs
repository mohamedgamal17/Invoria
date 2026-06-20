using FluentAssertions;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class CreateOrderReturnSagaActivityHandlerTests
{
    [Test]
    public async Task Publishes_OrderReturnRequestedIntegrationEvent_from_activity()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateOrderReturnSagaActivityHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CreateOrderReturnSagaActivityHandler>>());

        var lines = new List<OrderReturnLineModel>
        {
            new() { OrderItemId = "line-1", ProductId = "p1", Quantity = 1 }
        };

        await handler.Handle(new CreateOrderReturnSagaActivity("order-1", "alloc-1", lines));

        bus.Verify(
            b => b.Publish(
                It.Is<OrderReturnRequestedIntegrationEvent>(e =>
                    e.OrderId == "order-1" &&
                    e.AllocationId == "alloc-1" &&
                    e.Lines.Count == 1 &&
                    e.Lines[0].OrderItemId == "line-1" &&
                    e.Lines[0].ProductId == "p1" &&
                    e.Lines[0].Quantity == 1),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
