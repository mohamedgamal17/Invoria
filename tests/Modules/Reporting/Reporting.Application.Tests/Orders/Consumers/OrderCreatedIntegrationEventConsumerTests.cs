using System.Linq.Expressions;
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
public sealed class OrderCreatedIntegrationEventConsumerTests
{
    [Test]
    public async Task When_no_existing_row_Adds_ReportedOrder_with_lines()
    {
        var repo = new Mock<IReportingRepository<ReportedOrder>>();
        repo.Setup(r => r.SingleOrDefault(It.IsAny<Expression<Func<ReportedOrder, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportedOrder?)null);

        ReportedOrder? captured = null;
        repo.Setup(r => r.Add(It.IsAny<ReportedOrder>(), It.IsAny<CancellationToken>()))
            .Callback<ReportedOrder, CancellationToken>((o, _) => captured = o)
            .ReturnsAsync((ReportedOrder o, CancellationToken _) => o);

        var occurred = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var message = new OrderCreatedIntegrationEvent
        {
            OccurredOn = occurred,
            Order = new OrderIntegrationPayload
            {
                Id = "order-1",
                OrderNumber = "ON-9",
                CustomerId = "cust-1",
                OrderStatus = OrderStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 100m,
                AmountPaid = 0m,
                AmountOutstanding = 100m,
                Lines =
                [
                    new OrderLineModel
                    {
                        Id = "line-1",
                        ProductId = "p1",
                        Quantity = 2,
                        UnitPrice = 50m,
                        LineTotal = 100m
                    }
                ]
            }
        };

        var consumer = new OrderCreatedIntegrationEventConsumer(
            repo.Object,
            Mock.Of<ILogger<OrderCreatedIntegrationEventConsumer>>());

        await consumer.Handle(message);

        repo.Verify(r => r.Add(It.IsAny<ReportedOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(captured, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(captured!.Id, Is.EqualTo("order-1"));
            Assert.That(captured.OrderNumber, Is.EqualTo("ON-9"));
            Assert.That(captured.CustomerId, Is.EqualTo("cust-1"));
            Assert.That(captured.ReplicationVersion, Is.EqualTo(1));
            Assert.That(captured.SourceLastKnownAt, Is.EqualTo(occurred));
            Assert.That(captured.CreatedAt, Is.EqualTo(occurred));
            Assert.That(captured.Lines, Has.Count.EqualTo(1));
            Assert.That(captured.Lines[0].Id, Is.EqualTo("line-1"));
            Assert.That(captured.Lines[0].ReportedOrderId, Is.EqualTo("order-1"));
            Assert.That(captured.Lines[0].ProductId, Is.EqualTo("p1"));
            Assert.That(captured.Lines[0].Quantity, Is.EqualTo(2));
            Assert.That(captured.Lines[0].UnitPrice, Is.EqualTo(50m));
            Assert.That(captured.Lines[0].LineTotal, Is.EqualTo(100m));
        });
    }

    [Test]
    public async Task When_row_exists_does_not_call_Add()
    {
        var existing = new ReportedOrder
        {
            Id = "order-1",
            OrderNumber = "ON-9",
            CustomerId = "cust-1",
            OrderStatus = OrderStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 100m,
            AmountPaid = 0m,
            AmountOutstanding = 100m,
            ReplicationVersion = 1,
            SourceLastKnownAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var repo = new Mock<IReportingRepository<ReportedOrder>>();
        repo.Setup(r => r.SingleOrDefault(It.IsAny<Expression<Func<ReportedOrder, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var message = new OrderCreatedIntegrationEvent
        {
            OccurredOn = DateTimeOffset.UtcNow,
            Order = new OrderIntegrationPayload
            {
                Id = "order-1",
                OrderNumber = "ON-9",
                CustomerId = "cust-1",
                OrderStatus = OrderStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 100m,
                AmountPaid = 0m,
                AmountOutstanding = 100m,
                Lines = []
            }
        };

        var consumer = new OrderCreatedIntegrationEventConsumer(
            repo.Object,
            Mock.Of<ILogger<OrderCreatedIntegrationEventConsumer>>());

        await consumer.Handle(message);

        repo.Verify(r => r.Add(It.IsAny<ReportedOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
