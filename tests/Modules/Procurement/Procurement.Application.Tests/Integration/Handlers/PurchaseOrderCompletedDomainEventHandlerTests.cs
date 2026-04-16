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
            items:
            [
                new PurchaseOrderCompletedDomainEvent.Item(
                    PurchaseOrderItemId: "li1",
                    ProductId: "p1",
                    Quantity: 2,
                    UnitPrice: 10m,
                    SupplierProductCode: "SKU-1")
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
                    msg.Items[0].UnitPrice == 10m &&
                    msg.Items[0].SupplierProductCode == "SKU-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}

