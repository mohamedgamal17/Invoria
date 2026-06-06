using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Application.Allocations.Handlers;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class AllocationReleasedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_AllocationReleasedIntegrationEvent_with_allocation_and_order_ids()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new AllocationReleasedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AllocationReleasedDomainEventHandler>>());

        var allocation = Allocation.CreateForOrder("order-9", [("oi-1", "p-1", 3)]);
        allocation.ClearDomainEvents();
        SetAllocationAllocated(allocation);
        allocation.MarkAsReleased();
        var ev = allocation.DomainEvents.OfType<AllocationReleasedDomainEvent>().Single();

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<AllocationReleasedIntegrationEvent>(msg =>
                    msg.AllocationId == allocation.Id && msg.OrderId == "order-9"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    private static void SetAllocationAllocated(Allocation allocation)
    {
        var batch = new Invoria.Inventory.Domain.Batches.Batch("p-1", 10, 10m);
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(batch, "batch-1");

        foreach (var line in allocation.Lines)
        {
            line.RecordBatchAllocation(
                batch.AllocateForOrder(line.OrderItemId, line.QuantityRequested, DateTimeOffset.UtcNow));
            line.MarkAsAllocated();
        }

        allocation.MarkAsAllocated();
    }
}
