using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Commands.RecordPurchaseSalesMetrics;
using Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Consumers;
using Invoria.Procurement.Contracts.Events;
using MediatR;
using Moq;
using NUnit.Framework;

namespace Invoria.Procurement.Application.Tests.ReportPurchaseSalesMetrics.Consumers;

[TestFixture]
public class RecordPurchaseSalesMetricsIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_record_purchase_sales_metrics_command_from_message()
    {
        var completedAt = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RecordPurchaseSalesMetricsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new RecordPurchaseSalesMetricsIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RecordPurchaseSalesMetricsIntegrationEventConsumer>>());

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
                It.Is<RecordPurchaseSalesMetricsCommand>(c =>
                    c.PurchaseOrderId == "po-1" &&
                    c.OccurredOn == completedAt),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Throws_when_command_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RecordPurchaseSalesMetricsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Empty>(
                new InvalidOperationException("Could not persist metrics.")));

        var consumer = new RecordPurchaseSalesMetricsIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RecordPurchaseSalesMetricsIntegrationEventConsumer>>());

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