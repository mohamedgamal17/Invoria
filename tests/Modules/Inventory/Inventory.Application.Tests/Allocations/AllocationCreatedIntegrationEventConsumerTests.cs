using FluentAssertions;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Allocations.Consumers;
using Invoria.Inventory.Contracts.Allocations.Enums;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.BuildingBlocks.Domain.Primitives;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class AllocationCreatedIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_RequestAllocationCommand_from_event()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RequestAllocationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new AllocationCreatedIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AllocationCreatedIntegrationEventConsumer>>());

        var message = new AllocationCreatedIntegrationEvent
        {
            OccurredOn = DateTimeOffset.UtcNow,
            Allocation = new AllocationModel
            {
                Id = "alloc-1",
                OrderId = "order-1",
                Status = AllocationStatus.Pending,
                Lines = []
            }
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<RequestAllocationCommand>(c => c.AllocationId == "alloc-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
