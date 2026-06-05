using Invoria.Inventory.Application.Allocations.Handlers;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Moq;
using Rebus.Bus;
using ContractAllocationLineStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationLineStatus;
using ContractAllocationStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationStatus;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class AllocationInitiatedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_AllocationCreatedIntegrationEvent_with_allocation_snapshot()
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
                It.Is<AllocationCreatedIntegrationEvent>(msg =>
                    msg.Allocation.Id == allocation.Id &&
                    msg.Allocation.OrderId == "order-1" &&
                    msg.Allocation.Status == ContractAllocationStatus.Pending &&
                    msg.Allocation.Lines.Count == 1 &&
                    msg.Allocation.Lines[0].OrderItemId == "oi-1" &&
                    msg.Allocation.Lines[0].ProductId == "p-1" &&
                    msg.Allocation.Lines[0].QuantityRequested == 3 &&
                    msg.Allocation.Lines[0].Status == ContractAllocationLineStatus.Pending &&
                    msg.OccurredOn != default),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
