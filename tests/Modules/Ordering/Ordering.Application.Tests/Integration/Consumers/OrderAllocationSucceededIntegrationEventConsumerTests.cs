using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Application.Orders.Consumers;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Moq;

namespace Invoria.Ordering.Application.Tests.Integration.Consumers;

[TestFixture]
public class OrderAllocationSucceededIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_record_allocation_command_from_event_payload()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RecordOrderAllocationSucceededCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new OrderAllocationSucceededIntegrationEventConsumer(mediator.Object);

        var message = new OrderAllocationSucceededIntegrationEvent
        {
            OrderId = "order-alloc-1",
            OrderNumber = "ON-1",
            CustomerId = "cust-42",
            AllocatedAt = DateTimeOffset.UtcNow,
            AllocatedLines =
            [
                new OrderAllocationSucceededLineModel
                {
                    OrderItemId = "line-1",
                    ProductId = "p1",
                    Quantity = 3
                }
            ]
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<RecordOrderAllocationSucceededCommand>(c =>
                    c.OrderId == "order-alloc-1" &&
                    c.CustomerId == "cust-42"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
