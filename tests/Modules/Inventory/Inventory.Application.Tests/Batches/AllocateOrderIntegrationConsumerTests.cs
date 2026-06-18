using FluentAssertions;
using Invoria.Inventory.Application.Allocations.Commands.CreateAllocate;
using Invoria.Inventory.Application.Batches.Consumers;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class AllocateOrderIntegrationConsumerTests
{
    [Test]
    public async Task Sends_CreateAllocateCommand_from_event()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<CreateAllocateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new AllocateOrderIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AllocateOrderIntegrationEventConsumer>>());

        var message = new AllocateOrderIntegrationEvent
        {
            Id = "order-1",
            OrderNumber = "ON-9",
            CustomerId = "c1",
            Items = new List<AllocateOrderLineModel> { new() { Id = "i1", ProductId = "p1", Quantity = 2 } }
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<CreateAllocateCommand>(c =>
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
