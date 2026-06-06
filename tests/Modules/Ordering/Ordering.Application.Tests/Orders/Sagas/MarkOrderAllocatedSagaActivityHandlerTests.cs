using FluentAssertions;
using Invoria.Ordering.Application.Orders.Commands.MarkOrderAllocated;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using MediatR;
using Moq;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class MarkOrderAllocatedSagaActivityHandlerTests
{
    [Test]
    public async Task Sends_MarkOrderAllocatedCommand_from_activity()
    {
        var mediator = new Mock<IMediator>();

        var handler = new MarkOrderAllocatedSagaActivityHandler(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<MarkOrderAllocatedSagaActivityHandler>>());

        await handler.Handle(new MarkOrderAllocatedSagaActivity("order-1"));

        mediator.Verify(
            m => m.Send(
                It.Is<MarkOrderAllocatedCommand>(c => c.OrderId == "order-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
