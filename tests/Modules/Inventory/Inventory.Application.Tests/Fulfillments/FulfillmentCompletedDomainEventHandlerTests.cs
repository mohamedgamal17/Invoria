using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Application.Fulfillments.Handlers;
using Invoria.Inventory.Contracts.Events;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Fulfillments;
using Invoria.Inventory.Domain.Fulfillments.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Fulfillments;

[TestFixture]
public class FulfillmentCompletedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_FulfillmentCompletedIntegrationEvent_with_fulfillment_order_and_allocation_ids()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new FulfillmentCompletedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<FulfillmentCompletedDomainEventHandler>>());

        var allocation = CreateFullyAllocatedAllocation("order-1");
        var fulfillment = Fulfillment.CreateFromAllocation(allocation);
        fulfillment.ClearDomainEvents();
        fulfillment.RequestDispatch();
        fulfillment.ClearDomainEvents();
        fulfillment.Complete();
        var ev = fulfillment.DomainEvents.OfType<FulfillmentCompletedDomainEvent>().Single();

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<FulfillmentCompletedIntegrationEvent>(msg =>
                    msg.FulfillmentId == fulfillment.Id
                    && msg.OrderId == "order-1"
                    && msg.AllocationId == allocation.Id),
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
        allocation.TryMarkAsAllocated();
        return allocation;
    }
}
