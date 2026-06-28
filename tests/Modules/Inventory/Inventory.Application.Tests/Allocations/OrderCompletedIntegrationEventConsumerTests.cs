using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Allocations.Commands.CompleteAllocation;
using Invoria.Inventory.Application.Allocations.Consumers;
using Invoria.Ordering.Contracts.Orders.Events;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class OrderCompletedIntegrationEventConsumerTests
{
    [Test]
    public async Task Handle_sends_CompleteAllocationCommand_with_allocation_id()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<CompleteAllocationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new OrderCompletedIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderCompletedIntegrationEventConsumer>>());

        var message = new OrderCompletedIntegrationEvent
        {
            OrderId = "order-1",
            OccurredOn = DateTimeOffset.UtcNow,
            AllocationId = "alloc-1"
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<CompleteAllocationCommand>(cmd => cmd.AllocationId == "alloc-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
