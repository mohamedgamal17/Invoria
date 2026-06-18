using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Returns.Commands.ProcessImmediateReturn;
using Invoria.Inventory.Application.Returns.Consumers;
using Invoria.Inventory.Contracts.Returns.Events;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class ProcessImmediateReturnIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_ProcessImmediateReturnCommand_from_event()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ProcessImmediateReturnCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new ProcessImmediateReturnIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ProcessImmediateReturnIntegrationEventConsumer>>());

        var message = new ProcessImmediateReturnIntegrationEvent
        {
            ReturnId = "return-1"
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<ProcessImmediateReturnCommand>(c => c.ReturnId == "return-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
