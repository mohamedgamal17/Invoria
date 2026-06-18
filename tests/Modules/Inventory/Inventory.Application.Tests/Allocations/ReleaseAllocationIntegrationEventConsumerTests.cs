using FluentAssertions;
using Invoria.Inventory.Application.Allocations.Commands.ReleaseAllocation;
using Invoria.Inventory.Application.Allocations.Consumers;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.BuildingBlocks.Domain.Primitives;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class ReleaseAllocationIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_ReleaseAllocationCommand_from_event()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ReleaseAllocationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new ReleaseAllocationIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ReleaseAllocationIntegrationEventConsumer>>());

        var message = new ReleaseAllocationIntegrationEvent { AllocationId = "alloc-1" };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<ReleaseAllocationCommand>(c => c.AllocationId == "alloc-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
