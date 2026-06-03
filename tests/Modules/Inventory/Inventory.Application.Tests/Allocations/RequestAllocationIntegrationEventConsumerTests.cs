using FluentAssertions;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Allocations.Consumers;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.BuildingBlocks.Domain.Primitives;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class RequestAllocationIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_RequestAllocationCommand_from_event()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RequestAllocationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new RequestAllocationIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RequestAllocationIntegrationEventConsumer>>());

        var message = new RequestAllocationIntegrationEvent { AllocationId = "alloc-1" };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<RequestAllocationCommand>(c => c.AllocationId == "alloc-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
