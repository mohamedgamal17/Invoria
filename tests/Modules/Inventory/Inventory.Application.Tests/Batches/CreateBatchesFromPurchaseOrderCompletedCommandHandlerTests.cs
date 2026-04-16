using Invoria.Inventory.Application.Batches.Commands.CreateBatchesFromPurchaseOrderCompleted;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Moq;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class CreateBatchesFromPurchaseOrderCompletedCommandHandlerTests
{
    [Test]
    public async Task Creates_one_batch_per_purchase_item()
    {
        var batchRepository = new Mock<IInventoryRepository<Batch>>();
        batchRepository
            .Setup(r => r.Add(It.IsAny<Batch>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Batch b, CancellationToken _) => b);

        var handler = new CreateBatchesFromPurchaseOrderCompletedCommandHandler(batchRepository.Object);
        var command = new CreateBatchesFromPurchaseOrderCompletedCommand
        {
            PurchaseOrderId = "po-1",
            PurchaseNumber = "PO-0001",
            SupplierId = "sup-1",
            CompletedAt = DateTimeOffset.UtcNow,
            Items =
            [
                new CreateBatchesFromPurchaseOrderCompletedCommand.Item
                {
                    PurchaseOrderItemId = "poi-1",
                    ProductId = "p-1",
                    Quantity = 2,
                    UnitPrice = 10m
                },
                new CreateBatchesFromPurchaseOrderCompletedCommand.Item
                {
                    PurchaseOrderItemId = "poi-2",
                    ProductId = "p-2",
                    Quantity = 5,
                    UnitPrice = 25.5m
                }
            ]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        batchRepository.Verify(
            r => r.Add(
                It.Is<Batch>(b =>
                    b.ProductId == "p-1" &&
                    b.Quantity == 2 &&
                    b.PurchasePrice == 10m &&
                    b.PurchaseOrderItemId == "poi-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        batchRepository.Verify(
            r => r.Add(
                It.Is<Batch>(b =>
                    b.ProductId == "p-2" &&
                    b.Quantity == 5 &&
                    b.PurchasePrice == 25.5m &&
                    b.PurchaseOrderItemId == "poi-2"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Returns_failure_when_persisting_batch_fails()
    {
        var batchRepository = new Mock<IInventoryRepository<Batch>>();
        batchRepository
            .Setup(r => r.Add(It.IsAny<Batch>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Could not persist batch."));

        var handler = new CreateBatchesFromPurchaseOrderCompletedCommandHandler(batchRepository.Object);
        var command = new CreateBatchesFromPurchaseOrderCompletedCommand
        {
            PurchaseOrderId = "po-1",
            PurchaseNumber = "PO-0001",
            SupplierId = "sup-1",
            CompletedAt = DateTimeOffset.UtcNow,
            Items =
            [
                new CreateBatchesFromPurchaseOrderCompletedCommand.Item
                {
                    PurchaseOrderItemId = "poi-1",
                    ProductId = "p-1",
                    Quantity = 2,
                    UnitPrice = 10m
                }
            ]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Exception, Is.TypeOf<InvalidOperationException>());
        Assert.That(result.Exception!.Message, Does.Contain("Could not persist batch."));
    }
}
