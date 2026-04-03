using FluentAssertions;
using Invoria.Inventory.Application.Batches.Commands.DispatchOrder;
using Invoria.Inventory.Application.Batches.Consumers;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class OrderDispatchedIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_DispatchOrderCommand_from_message()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<DispatchOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new OrderDispatchedIntegrationEventConsumer(mediator.Object);

        var message = new OrderDispatchedIntegrationEvent
        {
            Id = "order-1",
            OrderNumber = "ON-1",
            CustomerId = "c1",
            DispatchedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItemModel> { new() { Id = "i1", ProductId = "p1", Quantity = 2 } }
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<DispatchOrderCommand>(c =>
                    c.Id == "order-1" &&
                    c.OrderNumber == "ON-1" &&
                    c.CustomerId == "c1" &&
                    c.Items.Count == 1 &&
                    c.Items[0].Id == "i1" &&
                    c.Items[0].ProductId == "p1" &&
                    c.Items[0].Quantity == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Throws_when_command_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<DispatchOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Empty>(new InvalidOperationException("No allocations.")));

        var consumer = new OrderDispatchedIntegrationEventConsumer(mediator.Object);

        var message = new OrderDispatchedIntegrationEvent
        {
            Id = "order-1",
            OrderNumber = "ON-1",
            CustomerId = "c1",
            DispatchedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItemModel> { new() { Id = "i1", ProductId = "p1", Quantity = 1 } }
        };

        var act = async () => await consumer.Handle(message);

        act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No allocations.");
    }
}
