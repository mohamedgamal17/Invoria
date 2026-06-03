using Invoria.Inventory.Application.Allocations.Handlers;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class AllocationFailedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_AllocationFailedIntegrationEvent_with_allocation_and_order_ids()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new AllocationFailedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AllocationFailedDomainEventHandler>>());

        var allocation = Allocation.CreateForOrder("order-9", [("oi-1", "p-1", 3)]);
        allocation.ClearDomainEvents();
        allocation.MarkAsFailed();
        var ev = allocation.DomainEvents.OfType<AllocationFailedDomainEvent>().Single();

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<AllocationFailedIntegrationEvent>(msg =>
                    msg.AllocationId == allocation.Id && msg.OrderId == "order-9"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
