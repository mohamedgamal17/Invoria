using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Invoria.Reporting.Application.Tests.Orders.Materialization.StatusSummary;

[TestFixture]
public sealed class ReportedOrderStatusSummaryRollupRefresherTests
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
    public async Task Refresh_replaces_rollups_with_grouped_counts_by_utc_day_and_status()
    {
        var dayA = DateTimeOffset.Parse("2026-04-10T22:00:00Z");
        var dayB = DateTimeOffset.Parse("2026-04-11T06:00:00Z");

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
                CreatedAt = dayA,
                SourceLastKnownAt = dayA
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
                CreatedAt = dayA,
                SourceLastKnownAt = dayA
            },
            new ReportedOrder
            {
                Id = "c",
                OrderNumber = "c",
                CustomerId = "c",
                OrderStatus = OrderStatus.Accepted,
                FullfillmentStatus = FullfillmentStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 1m,
                AmountPaid = 0m,
                AmountOutstanding = 1m,
                ReplicationVersion = 1,
                CreatedAt = dayB,
                SourceLastKnownAt = dayB
            });
        await db.SaveChangesAsync();

        var refresher = new ReportedOrderStatusSummaryRollupRefresher(db, NullLogger<ReportedOrderStatusSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var rollup = await db.ReportedOrderStatusByDays.AsNoTracking().OrderBy(r => r.DayUtc).ThenBy(r => r.OrderStatus).ToListAsync();

        Assert.That(rollup, Has.Count.EqualTo(2));
        var dayOnlyA = DateOnly.FromDateTime(dayA.UtcDateTime);
        var dayOnlyB = DateOnly.FromDateTime(dayB.UtcDateTime);

        Assert.Multiple(() =>
        {
            Assert.That(rollup[0].DayUtc, Is.EqualTo(dayOnlyA));
            Assert.That(rollup[0].OrderStatus, Is.EqualTo(OrderStatus.Pending));
            Assert.That(rollup[0].Count, Is.EqualTo(2));

            Assert.That(rollup[1].DayUtc, Is.EqualTo(dayOnlyB));
            Assert.That(rollup[1].OrderStatus, Is.EqualTo(OrderStatus.Accepted));
            Assert.That(rollup[1].Count, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Second_refresh_replaces_previous_rollups()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var t = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        db.ReportedOrders.Add(new ReportedOrder
        {
            Id = "x",
            OrderNumber = "x",
            CustomerId = "c",
            OrderStatus = OrderStatus.Pending,
            FullfillmentStatus = FullfillmentStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 1m,
            AmountPaid = 0m,
            AmountOutstanding = 1m,
            ReplicationVersion = 1,
            CreatedAt = t,
            SourceLastKnownAt = t
        });
        await db.SaveChangesAsync();

        var refresher = new ReportedOrderStatusSummaryRollupRefresher(db, NullLogger<ReportedOrderStatusSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);
        Assert.That(await db.ReportedOrderStatusByDays.CountAsync(), Is.EqualTo(1));

        db.ReportedOrders.RemoveRange(db.ReportedOrders);
        await db.SaveChangesAsync();

        await refresher.RefreshAsync(CancellationToken.None);
        Assert.That(await db.ReportedOrderStatusByDays.CountAsync(), Is.EqualTo(0));
    }
}
