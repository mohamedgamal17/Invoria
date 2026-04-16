using Invoria.Inventory.Application.Batches.Consumers;
using Invoria.Inventory.Application.Batches.Commands.CreateBatchesFromPurchaseOrderCompleted;
using Invoria.Procurement.Contracts.Events;
using Invoria.Procurement.Contracts.Models;
using MediatR;
using Moq;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class PurchaseOrderCompletedIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_create_batches_command_from_message()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<CreateBatchesFromPurchaseOrderCompletedCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Invoria.BuildingBlocks.Domain.Primitives.Result.Success(Invoria.BuildingBlocks.Domain.Primitives.Empty.Value));

        var consumer = new PurchaseOrderCompletedIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PurchaseOrderCompletedIntegrationEventConsumer>>());

        var message = new PurchaseOrderCompletedIntegrationEvent
        {
            PurchaseOrderId = "po-1",
            PurchaseNumber = "PO-0001",
            SupplierId = "sup-1",
            CompletedAt = DateTimeOffset.UtcNow,
            Items =
            [
                new PurchaseOrderItemModel
                {
                    PurchaseOrderItemId = "poi-1",
                    ProductId = "p-1",
                    Quantity = 2,
                    UnitPrice = 10m
                },
                new PurchaseOrderItemModel
                {
                    PurchaseOrderItemId = "poi-2",
                    ProductId = "p-2",
                    Quantity = 5,
                    UnitPrice = 25.5m
                }
            ]
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<CreateBatchesFromPurchaseOrderCompletedCommand>(c =>
                    c.PurchaseOrderId == "po-1" &&
                    c.PurchaseNumber == "PO-0001" &&
                    c.SupplierId == "sup-1" &&
                    c.Items.Count == 2 &&
                    c.Items[0].PurchaseOrderItemId == "poi-1" &&
                    c.Items[0].ProductId == "p-1" &&
                    c.Items[0].Quantity == 2 &&
                    c.Items[0].UnitPrice == 10m &&
                    c.Items[1].PurchaseOrderItemId == "poi-2" &&
                    c.Items[1].ProductId == "p-2" &&
                    c.Items[1].Quantity == 5 &&
                    c.Items[1].UnitPrice == 25.5m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Throws_when_command_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<CreateBatchesFromPurchaseOrderCompletedCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Invoria.BuildingBlocks.Domain.Primitives.Result.Failure<Invoria.BuildingBlocks.Domain.Primitives.Empty>(new InvalidOperationException("Could not persist batch.")));

        var consumer = new PurchaseOrderCompletedIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PurchaseOrderCompletedIntegrationEventConsumer>>());

        var message = new PurchaseOrderCompletedIntegrationEvent
        {
            PurchaseOrderId = "po-1",
            PurchaseNumber = "PO-0001",
            SupplierId = "sup-1",
            CompletedAt = DateTimeOffset.UtcNow,
            Items =
            [
                new PurchaseOrderItemModel
                {
                    PurchaseOrderItemId = "poi-1",
                    ProductId = "p-1",
                    Quantity = 2,
                    UnitPrice = 10m
                }
            ]
        };

        var act = async () => await consumer.Handle(message);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await act());
    }
}
