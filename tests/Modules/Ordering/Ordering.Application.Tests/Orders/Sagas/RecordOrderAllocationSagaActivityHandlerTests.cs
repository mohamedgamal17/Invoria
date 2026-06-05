using FluentAssertions;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocation;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using MediatR;
using Moq;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class RecordOrderAllocationSagaActivityHandlerTests
{
    [Test]
    public async Task Sends_RecordOrderAllocationCommand_from_activity()
    {
        var mediator = new Mock<IMediator>();

        var handler = new RecordOrderAllocationSagaActivityHandler(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RecordOrderAllocationSagaActivityHandler>>());

        await handler.Handle(new RecordOrderAllocationSagaActivity("order-1", "alloc-1"));

        mediator.Verify(
            m => m.Send(
                It.Is<RecordOrderAllocationCommand>(c =>
                    c.OrderId == "order-1" &&
                    c.AllocationId == "alloc-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
