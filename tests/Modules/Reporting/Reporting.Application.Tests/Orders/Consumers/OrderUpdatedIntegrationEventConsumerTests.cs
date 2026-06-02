using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Reporting.Application.Orders.Consumers;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Invoria.Reporting.Application.Tests.Orders.Consumers;

[TestFixture]
public sealed class OrderUpdatedIntegrationEventConsumerTests
{
    private static OrderUpdatedIntegrationEvent BuildMessage(DateTimeOffset occurred) =>
        new()
        {
            OccurredOn = occurred,
            Order = new OrderIntegrationPayload
            {
                Id = "order-1",
                OrderNumber = "ON-9",
                CustomerId = "cust-1",
                OrderStatus = OrderStatus.Processing,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 200m,
                AmountPaid = 0m,
                AmountOutstanding = 200m,
                Lines =
                [
                    new OrderLineModel
                    {
                        Id = "line-2",
                        ProductId = "p2",
                        Quantity = 1,
                        UnitPrice = 200m,
                        LineTotal = 200m
                    }
                ]
            }
        };

    [Test]
    public async Task When_row_exists_applies_snapshot_and_replaces_lines()
    {
        var occurred = DateTimeOffset.Parse("2026-05-10T12:00:00Z");
        var prior = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        var existing = new ReportedOrder
        {
            Id = "order-1",
            OrderNumber = "ON-1",
            CustomerId = "cust-old",
            OrderStatus = OrderStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 100m,
            AmountPaid = 0m,
            AmountOutstanding = 100m,
            ReplicationVersion = 1,
            SourceLastKnownAt = prior,
            CreatedAt = prior,
            Lines =
            [
                new ReportedOrderLine
                {
                    Id = "line-1",
                    ReportedOrderId = "order-1",
                    ProductId = "p1",
                    Quantity = 2,
                    UnitPrice = 50m,
                    LineTotal = 100m,
                    ReportedOrder = null
                }
            ]
        };

        var repo = new Mock<IReportedOrderRepository>();
        repo.Setup(r => r.GetByIdWithGraphAsync("order-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        ReportedOrder? upserted = null;
        repo.Setup(r => r.UpsertGraphAsync(It.IsAny<ReportedOrder>(), It.IsAny<CancellationToken>()))
            .Callback<ReportedOrder, CancellationToken>((o, _) => upserted = o)
            .Returns(Task.CompletedTask);

        var consumer = new OrderUpdatedIntegrationEventConsumer(
            repo.Object,
            Mock.Of<ILogger<OrderUpdatedIntegrationEventConsumer>>());

        await consumer.Handle(BuildMessage(occurred));

        repo.Verify(r => r.UpsertGraphAsync(It.IsAny<ReportedOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(upserted, Is.SameAs(existing));
        Assert.Multiple(() =>
        {
            Assert.That(upserted!.OrderNumber, Is.EqualTo("ON-9"));
            Assert.That(upserted.CustomerId, Is.EqualTo("cust-1"));
            Assert.That(upserted.OrderStatus, Is.EqualTo(OrderStatus.Processing));
            Assert.That(upserted.TotalOrderAmount, Is.EqualTo(200m));
            Assert.That(upserted.ReplicationVersion, Is.EqualTo(2));
            Assert.That(upserted.SourceLastKnownAt, Is.EqualTo(occurred));
            Assert.That(upserted.LastModifiedAt, Is.EqualTo(occurred));
            Assert.That(upserted.Lines, Has.Count.EqualTo(1));
            Assert.That(upserted.Lines[0].Id, Is.EqualTo("line-2"));
            Assert.That(upserted.Lines[0].ProductId, Is.EqualTo("p2"));
        });
    }

    [Test]
    public async Task When_occurred_on_is_stale_does_not_call_UpsertGraphAsync()
    {
        var occurred = DateTimeOffset.Parse("2026-05-01T10:00:00Z");
        var newer = DateTimeOffset.Parse("2026-05-10T12:00:00Z");

        var existing = new ReportedOrder
        {
            Id = "order-1",
            OrderNumber = "ON-9",
            CustomerId = "cust-1",
            OrderStatus = OrderStatus.Processing,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 200m,
            AmountPaid = 0m,
            AmountOutstanding = 200m,
            ReplicationVersion = 2,
            SourceLastKnownAt = newer,
            CreatedAt = DateTimeOffset.Parse("2026-04-01T00:00:00Z"),
            Lines = []
        };

        var repo = new Mock<IReportedOrderRepository>();
        repo.Setup(r => r.GetByIdWithGraphAsync("order-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var consumer = new OrderUpdatedIntegrationEventConsumer(
            repo.Object,
            Mock.Of<ILogger<OrderUpdatedIntegrationEventConsumer>>());

        await consumer.Handle(BuildMessage(occurred));

        repo.Verify(r => r.UpsertGraphAsync(It.IsAny<ReportedOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task When_row_missing_inserts_via_UpsertGraphAsync()
    {
        var occurred = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        var repo = new Mock<IReportedOrderRepository>();
        repo.Setup(r => r.GetByIdWithGraphAsync("order-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportedOrder?)null);

        ReportedOrder? upserted = null;
        repo.Setup(r => r.UpsertGraphAsync(It.IsAny<ReportedOrder>(), It.IsAny<CancellationToken>()))
            .Callback<ReportedOrder, CancellationToken>((o, _) => upserted = o)
            .Returns(Task.CompletedTask);

        var consumer = new OrderUpdatedIntegrationEventConsumer(
            repo.Object,
            Mock.Of<ILogger<OrderUpdatedIntegrationEventConsumer>>());

        await consumer.Handle(BuildMessage(occurred));

        repo.Verify(r => r.UpsertGraphAsync(It.IsAny<ReportedOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(upserted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(upserted!.Id, Is.EqualTo("order-1"));
            Assert.That(upserted.ReplicationVersion, Is.EqualTo(1));
            Assert.That(upserted.CreatedAt, Is.EqualTo(occurred));
            Assert.That(upserted.SourceLastKnownAt, Is.EqualTo(occurred));
            Assert.That(upserted.Lines, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task When_SourceLastKnownAt_is_null_applies_update()
    {
        var occurred = DateTimeOffset.Parse("2026-05-10T12:00:00Z");

        var existing = new ReportedOrder
        {
            Id = "order-1",
            OrderNumber = "ON-1",
            CustomerId = "cust-1",
            OrderStatus = OrderStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 100m,
            AmountPaid = 0m,
            AmountOutstanding = 100m,
            ReplicationVersion = 5,
            SourceLastKnownAt = null,
            CreatedAt = DateTimeOffset.Parse("2026-04-01T00:00:00Z"),
            Lines = []
        };

        var repo = new Mock<IReportedOrderRepository>();
        repo.Setup(r => r.GetByIdWithGraphAsync("order-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var consumer = new OrderUpdatedIntegrationEventConsumer(
            repo.Object,
            Mock.Of<ILogger<OrderUpdatedIntegrationEventConsumer>>());

        await consumer.Handle(BuildMessage(occurred));

        repo.Verify(r => r.UpsertGraphAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(existing.ReplicationVersion, Is.EqualTo(6));
        Assert.That(existing.SourceLastKnownAt, Is.EqualTo(occurred));
    }
}
