using FluentAssertions;
using Invoria.Inventory.Application.Fulfillments.Commands.DispatchFulfillment;
using Invoria.Inventory.Application.Fulfillments.Consumers;
using Invoria.Inventory.Contracts.Events;
using Invoria.BuildingBlocks.Domain.Primitives;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Fulfillments;

[TestFixture]
public class DispatchFulfillmentIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_DispatchFulfillmentCommand_from_event()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<DispatchFulfillmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new DispatchFulfillmentIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<DispatchFulfillmentIntegrationEventConsumer>>());

        var message = new DispatchFulfillmentIntegrationEvent
        {
            FulfillmentId = "fulfillment-1",
            OrderId = "order-1",
            AllocationId = "alloc-1"
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<DispatchFulfillmentCommand>(c => c.FulfillmentId == "fulfillment-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
