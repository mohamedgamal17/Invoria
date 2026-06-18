using Invoria.Inventory.Application.Returns.Handlers;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Domain.Returns.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class ReturnApprovedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_ProcessImmediateReturnIntegrationEvent_for_immediate_return()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new ReturnApprovedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ReturnApprovedDomainEventHandler>>());

        var immediateReturn = ImmediateReturn.Create(
            "allocation-1",
            "order-1",
            [ReturnLine.Create("oi-1", "p-1", 3)]);
        immediateReturn.Approve();
        var ev = immediateReturn.DomainEvents.OfType<ReturnApprovedDomainEvent>().Single();

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<ProcessImmediateReturnIntegrationEvent>(msg =>
                    msg.ReturnId == immediateReturn.Id),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Does_not_publish_when_return_type_is_not_immediate()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new ReturnApprovedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ReturnApprovedDomainEventHandler>>());

        var @return = UnsupportedTypeReturn.Create([ReturnLine.Create("oi-1", "p-1", 1)]);
        @return.Approve();
        var ev = @return.DomainEvents.OfType<ReturnApprovedDomainEvent>().Single();

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    private sealed class UnsupportedTypeReturn : Return
    {
        public static UnsupportedTypeReturn Create(IEnumerable<ReturnLine> returnLines) =>
            new(returnLines);

        private UnsupportedTypeReturn(IEnumerable<ReturnLine> returnLines)
            : base(returnLines, (ReturnType)99)
        {
        }
    }
}
