using Invoria.Inventory.Application.Returns.Handlers;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Domain.Returns.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class ImmediateReturnCreatedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_ImmediateReturnCreatedIntegrationEvent_with_return_snapshot()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new ImmediateReturnCreatedDomainEventHandler(
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ImmediateReturnCreatedDomainEventHandler>>());

        var immediateReturn = ImmediateReturn.Create(
            "allocation-1",
            "order-1",
            [ReturnLine.Create("oi-1", "p-1", 3)]);
        var ev = immediateReturn.DomainEvents.OfType<ImmediateReturnCreatedDomainEvent>().Single();

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<ImmediateReturnCreatedIntegrationEvent>(msg =>
                    msg.ReturnId == immediateReturn.Id &&
                    msg.OrderId == "order-1" &&
                    msg.AllocationId == "allocation-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
