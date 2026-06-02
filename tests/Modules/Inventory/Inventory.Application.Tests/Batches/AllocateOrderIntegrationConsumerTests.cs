using FluentAssertions;
using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Inventory.Application.Batches.Consumers;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class AllocateOrderIntegrationConsumerTests
{
    [Test]
    public async Task Sends_AllocateOrderCommand_from_event()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<AllocateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new AllocateOrderIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AllocateOrderIntegrationEventConsumer>>());

        var message = new AllocateOrderIntegrationEvent
        {
            Id = "order-1",
            OrderNumber = "ON-9",
            CustomerId = "c1",
            Items = new List<OrderItemModel> { new() { Id = "i1", ProductId = "p1", Quantity = 2 } }
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<AllocateOrderCommand>(c =>
                    c.Id == "order-1" &&
                    c.OrderNumber == "ON-9" &&
                    c.CustomerId == "c1" &&
                    c.Items.Count == 1 &&
                    c.Items[0].Id == "i1" &&
                    c.Items[0].ProductId == "p1" &&
                    c.Items[0].Quantity == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
