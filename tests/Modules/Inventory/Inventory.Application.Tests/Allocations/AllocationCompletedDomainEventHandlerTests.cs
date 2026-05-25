using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Application.Allocations.Handlers;
using Invoria.Inventory.Contracts.Events;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Invoria.Inventory.Domain.Batches;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class AllocationCompletedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_AllocationSucceededIntegrationEvent_with_allocation_and_order_ids()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new AllocationCompletedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AllocationCompletedDomainEventHandler>>());

        var allocation = CreateFullyAllocatedAllocation("order-1");
        allocation.ClearDomainEvents();
        allocation.TryMarkAsAllocated();
        var ev = allocation.DomainEvents.OfType<AllocationCompletedDomainEvent>().Single();

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<AllocationSucceededIntegrationEvent>(msg =>
                    msg.AllocationId == allocation.Id && msg.OrderId == "order-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    private static Allocation CreateFullyAllocatedAllocation(string orderId)
    {
        var allocation = Allocation.CreateForOrder(orderId, [("oi-1", "p-1", 2)]);
        var line = allocation.Lines.Single();
        var batch = new Batch("p-1", 10, 10m);
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();
        return allocation;
    }
}
