using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Batches.Consumers;
using Invoria.Inventory.Application.Batches.Commands.ReleaseOrderAllocations;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using MediatR;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class ReleaseOrderAllocationsIntegrationConsumerTests
{
    [Test]
    public async Task Handle_publishes_OrderReopenInventoryReleased_when_release_succeeds()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ReleaseOrderAllocationsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var consumer = new ReleaseOrderAllocationsIntegrationEventConsumer(mediator.Object, bus.Object);
        var message = new ReleaseOrderAllocationsIntegrationEvent
        {
            Id = "order-1",
            OrderNumber = "ON-9",
            CustomerId = "c1",
            Items = new List<OrderItemModel> { new() { Id = "i1", ProductId = "p1", Quantity = 1 } }
        };

        await consumer.Handle(message);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderReopenInventoryReleasedIntegrationEvent>(e =>
                    e.OrderId == "order-1" &&
                    e.OrderNumber == "ON-9" &&
                    e.CustomerId == "c1" &&
                    e.ReleasedAt != default),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_publishes_OrderRefusalInventoryReleased_when_release_succeeds_and_reason_is_refusal()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ReleaseOrderAllocationsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var consumer = new ReleaseOrderAllocationsIntegrationEventConsumer(mediator.Object, bus.Object);
        var message = new ReleaseOrderAllocationsIntegrationEvent
        {
            Id = "order-1",
            OrderNumber = "ON-9",
            CustomerId = "c1",
            ReleaseReason = AllocationReleaseReason.Refusal,
            Items = new List<OrderItemModel> { new() { Id = "i1", ProductId = "p1", Quantity = 1 } }
        };

        await consumer.Handle(message);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderRefusalInventoryReleasedIntegrationEvent>(e =>
                    e.OrderId == "order-1" &&
                    e.OrderNumber == "ON-9" &&
                    e.CustomerId == "c1" &&
                    e.ReleasedAt != default),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
