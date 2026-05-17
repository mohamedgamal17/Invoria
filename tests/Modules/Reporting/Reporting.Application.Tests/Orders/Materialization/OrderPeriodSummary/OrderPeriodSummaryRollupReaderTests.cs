using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Invoria.Reporting.Infrastructure.Orders.Materialization.OrderPeriodSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Invoria.Reporting.Application.Tests.Orders.Materialization.OrderPeriodSummary;

[TestFixture]
public sealed class OrderPeriodSummaryRollupReaderTests
{
    private static ReportingDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ReportingDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var hookEngine = new Mock<IDbHookEngine>();
        hookEngine
            .Setup(h => h.RunBeforeSaveAsync(It.IsAny<DbContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        hookEngine
            .Setup(h => h.RunAfterSaveAsync(It.IsAny<DbContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new ReportingDbContext(options, hookEngine.Object);
    }

    [Test]
    public async Task GetPeriodSummariesPageAsync_returns_total_and_slice_in_order()
    {
        var d1 = DateTimeOffset.Parse("2026-05-01T10:00:00Z");
        var d2 = DateTimeOffset.Parse("2026-05-02T10:00:00Z");
        var d3 = DateTimeOffset.Parse("2026-05-03T10:00:00Z");

        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        db.ReportedOrders.AddRange(
            new ReportedOrder
            {
                Id = "a",
                OrderNumber = "a",
                CustomerId = "c",
                OrderStatus = OrderStatus.Pending,
                FullfillmentStatus = FullfillmentStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 1m,
                AmountPaid = 0m,
                AmountOutstanding = 1m,
                ReplicationVersion = 1,
                CreatedAt = d1,
                SourceLastKnownAt = d1
            },
            new ReportedOrder
            {
                Id = "b",
                OrderNumber = "b",
                CustomerId = "c",
                OrderStatus = OrderStatus.Pending,
                FullfillmentStatus = FullfillmentStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 1m,
                AmountPaid = 0m,
                AmountOutstanding = 1m,
                ReplicationVersion = 1,
                CreatedAt = d2,
                SourceLastKnownAt = d2
            },
            new ReportedOrder
            {
                Id = "c",
                OrderNumber = "c",
                CustomerId = "c",
                OrderStatus = OrderStatus.Pending,
                FullfillmentStatus = FullfillmentStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 1m,
                AmountPaid = 0m,
                AmountOutstanding = 1m,
                ReplicationVersion = 1,
                CreatedAt = d3,
                SourceLastKnownAt = d3
            });
        await db.SaveChangesAsync();

        var refresher = new OrderPeriodSummaryRollupRefresher(
            db,
            NullLogger<OrderPeriodSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var reader = new OrderPeriodSummaryRollupReader(db);
        var fromDay = new DateOnly(2026, 5, 1);
        var toDay = new DateOnly(2026, 5, 31);

        var (items, total) = await reader.GetPeriodSummariesPageAsync(
            OrderPeriodSummaryGranularity.Day,
            fromDay,
            toDay,
            skip: 1,
            take: 1,
            CancellationToken.None);

        Assert.That(total, Is.EqualTo(3));
        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].PeriodKey, Is.EqualTo("2026-05-02"));
    }
}
