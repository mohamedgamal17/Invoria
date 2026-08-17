using Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Commands.RecordPurchaseOrdersCompletedMetrics;
using Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Consumers;
using Invoria.Procurement.Contracts.Events;
using MediatR;
using Moq;
using NUnit.Framework;

namespace Invoria.Procurement.Application.Tests.ReportPurchaseOrdersCompletedMetrics.Consumers;

[TestFixture]
public class RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_record_purchase_orders_completed_metrics_command_from_message()
    {
        var completedAt = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RecordPurchaseOrdersCompletedMetricsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Invoria.BuildingBlocks.Domain.Primitives.Result.Success(Invoria.BuildingBlocks.Domain.Primitives.Empty.Value));

        var consumer = new RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer>>());

        var message = new PurchaseOrderCompletedIntegrationEvent
        {
            PurchaseOrderId = "po-1",
            PurchaseNumber = "PO-0001",
            SupplierId = "sup-1",
            CompletedAt = completedAt,
            Items = new List<Invoria.Procurement.Contracts.Models.PurchaseOrderItemModel>()
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<RecordPurchaseOrdersCompletedMetricsCommand>(c =>
                    c.OccurredOn == completedAt),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Throws_when_command_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RecordPurchaseOrdersCompletedMetricsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Invoria.BuildingBlocks.Domain.Primitives.Result.Failure<Invoria.BuildingBlocks.Domain.Primitives.Empty>(
                new InvalidOperationException("Could not persist metrics.")));

        var consumer = new RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer>>());

        var message = new PurchaseOrderCompletedIntegrationEvent
        {
            PurchaseOrderId = "po-1",
            PurchaseNumber = "PO-0001",
            SupplierId = "sup-1",
            CompletedAt = DateTimeOffset.UtcNow,
            Items = new List<Invoria.Procurement.Contracts.Models.PurchaseOrderItemModel>()
        };

        Assert.ThrowsAsync<InvalidOperationException>(async () => await consumer.Handle(message));
    }
}