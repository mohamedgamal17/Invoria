using Invoria.Inventory.Application.Allocations.Handlers;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class AllocationInitiatedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_RequestAllocationIntegrationEvent_with_allocation_id()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new AllocationInitiatedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AllocationInitiatedDomainEventHandler>>());

        var allocation = Allocation.CreateForOrder(
            "order-1",
            [("oi-1", "p-1", 3)]);
        var ev = allocation.DomainEvents.OfType<AllocationInitiatedDomainEvent>().Single();

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<RequestAllocationIntegrationEvent>(msg => msg.AllocationId == allocation.Id),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
