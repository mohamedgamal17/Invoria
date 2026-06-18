using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;
using Invoria.Inventory.Application.Returns.Consumers;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Inventory.Contracts.Returns.Models;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class CreateImmediateReturnIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_CreateImmediateReturnCommand_from_event()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<CreateImmediateReturnCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new CreateImmediateReturnIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CreateImmediateReturnIntegrationEventConsumer>>());

        var message = new CreateImmediateReturnIntegrationEvent
        {
            OrderId = "order-1",
            AllocationId = "allocation-1",
            Lines =
            [
                new ReturnLineModel
                {
                    OrderItemId = "oi-1",
                    ProductId = "p-1",
                    Quantity = 2
                }
            ]
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<CreateImmediateReturnCommand>(c =>
                    c.OrderId == "order-1" &&
                    c.AllocationId == "allocation-1" &&
                    c.Lines.Count == 1 &&
                    c.Lines[0].OrderItemId == "oi-1" &&
                    c.Lines[0].ProductId == "p-1" &&
                    c.Lines[0].Quantity == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
