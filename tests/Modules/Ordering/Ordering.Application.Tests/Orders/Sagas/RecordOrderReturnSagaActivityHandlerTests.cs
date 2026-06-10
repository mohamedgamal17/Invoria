using FluentAssertions;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderReturn;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using MediatR;
using Moq;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class RecordOrderReturnSagaActivityHandlerTests
{
    [Test]
    public async Task Sends_RecordOrderReturnCommand_from_activity()
    {
        var mediator = new Mock<IMediator>();

        var handler = new RecordOrderReturnSagaActivityHandler(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RecordOrderReturnSagaActivityHandler>>());

        await handler.Handle(new RecordOrderReturnSagaActivity("order-1", "return-1"));

        mediator.Verify(
            m => m.Send(
                It.Is<RecordOrderReturnCommand>(c =>
                    c.OrderId == "order-1" &&
                    c.ReturnId == "return-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
