using Invoria.Ordering.Application.Orders.Handlers;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Domain.Orders.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Handlers;

[TestFixture]
public class OrderRefusalReleaseRequestedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_ReleaseOrderAllocationsIntegrationEvent_with_refusal_reason()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new OrderRefusalReleaseRequestedDomainEventHandler(bus.Object);
        var ev = new OrderRefusalReleaseRequestedDomainEvent(
            "o1",
            "ON-1",
            "c1",
            new[]
            {
                new OrderDispatchedLine("i1", "p1", 2)
            });

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<ReleaseOrderAllocationsIntegrationEvent>(e =>
                    e.Id == "o1" &&
                    e.OrderNumber == "ON-1" &&
                    e.CustomerId == "c1" &&
                    e.ReleaseReason == AllocationReleaseReason.Refusal &&
                    e.Items.Count == 1 &&
                    e.Items[0].Id == "i1" &&
                    e.Items[0].ProductId == "p1" &&
                    e.Items[0].Quantity == 2),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
