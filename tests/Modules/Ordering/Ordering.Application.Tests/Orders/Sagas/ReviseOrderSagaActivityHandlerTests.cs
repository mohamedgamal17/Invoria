using FluentAssertions;
using Invoria.Ordering.Application.Orders.Commands.ReviseOrder;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using MediatR;
using Moq;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class ReviseOrderSagaActivityHandlerTests
{
    [Test]
    public async Task Sends_ReviseOrderCommand_from_activity()
    {
        var mediator = new Mock<IMediator>();

        var handler = new ReviseOrderSagaActivityHandler(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ReviseOrderSagaActivityHandler>>());

        await handler.Handle(new ReviseOrderSagaActivity("order-1"));

        mediator.Verify(
            m => m.Send(
                It.Is<ReviseOrderCommand>(c => c.OrderId == "order-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
