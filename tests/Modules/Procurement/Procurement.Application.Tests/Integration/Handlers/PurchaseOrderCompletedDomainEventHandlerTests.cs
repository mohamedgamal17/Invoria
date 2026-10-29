using Invoria.Financial.Contracts.Payables.Events;
using Invoria.Procurement.Application.PurchaseOrders.Handlers;
using Invoria.Procurement.Contracts.Events;
using Invoria.Procurement.Domain.PurchaseOrders.Events;
using Moq;
using Rebus.Bus;

namespace Invoria.Procurement.Application.Tests.Integration.Handlers;

[TestFixture]
public class PurchaseOrderCompletedDomainEventHandlerTests
{
    [Test]
    public async Task Publishes_PurchaseOrderCompletedIntegrationEvent_with_items()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new PurchaseOrderCompletedDomainEventHandler(bus.Object);

        var ev = new PurchaseOrderCompletedDomainEvent(
            purchaseOrderId: "po1",
            purchaseNumber: "PO-0001",
            supplierId: "sup1",
            completedAt: DateTimeOffset.UtcNow,
            totalAmount: 20m,
            items:
            [
                new PurchaseOrderCompletedDomainEvent.Item(
                    PurchaseOrderItemId: "li1",
                    ProductId: "p1",
                    Quantity: 2,
                    UnitPrice: 10m)
            ]);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<PurchaseOrderCompletedIntegrationEvent>(msg =>
                    msg.PurchaseOrderId == "po1" &&
                    msg.PurchaseNumber == "PO-0001" &&
                    msg.SupplierId == "sup1" &&
                    msg.Items.Count == 1 &&
                    msg.Items[0].PurchaseOrderItemId == "li1" &&
                    msg.Items[0].ProductId == "p1" &&
                    msg.Items[0].Quantity == 2 &&
                    msg.Items[0].UnitPrice == 10m),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Publishes_CreatePurchaseOrderPayableIntegrationEvent_with_total_amount()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new PurchaseOrderCompletedDomainEventHandler(bus.Object);

        var ev = new PurchaseOrderCompletedDomainEvent(
            purchaseOrderId: "po1",
            purchaseNumber: "PO-0001",
            supplierId: "sup1",
            completedAt: DateTimeOffset.UtcNow,
            totalAmount: 250.50m,
            items: []);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.Is<CreatePurchaseOrderPayableIntegrationEvent>(msg =>
                    msg.PurchaseOrderId == "po1" &&
                    msg.SupplierId == "sup1" &&
                    msg.Amount == 250.50m),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Does_not_publish_payable_when_total_amount_is_zero()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var handler = new PurchaseOrderCompletedDomainEventHandler(bus.Object);

        var ev = new PurchaseOrderCompletedDomainEvent(
            purchaseOrderId: "po1",
            purchaseNumber: "PO-0001",
            supplierId: "sup1",
            completedAt: DateTimeOffset.UtcNow,
            totalAmount: 0m,
            items: []);

        await handler.Handle(ev, CancellationToken.None);

        bus.Verify(
            b => b.Publish(
                It.IsAny<CreatePurchaseOrderPayableIntegrationEvent>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }
}
