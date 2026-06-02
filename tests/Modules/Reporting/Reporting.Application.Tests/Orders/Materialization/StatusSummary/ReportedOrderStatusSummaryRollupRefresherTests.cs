using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Reporting.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Tests.Orders.Materialization.StatusSummary;

[TestFixture]
public sealed class ReportedOrderStatusSummaryRollupRefresherTests : ReportedOrderStatusSummaryRollupRefresherTestFixture
{
    [Test]
    public async Task Refresh_replaces_rollups_with_grouped_counts_by_utc_day_and_status()
    {
        var dayA = DateTimeOffset.Parse("2026-04-10T22:00:00Z");
        var dayB = DateTimeOffset.Parse("2026-04-11T06:00:00Z");

        Db.ReportedOrders.AddRange(
            new ReportedOrder
            {
                Id = "a",
                OrderNumber = "a",
                CustomerId = "c",
                OrderStatus = OrderStatus.Pending,
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
                OrderStatus = OrderStatus.Processing,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 1m,
                AmountPaid = 0m,
                AmountOutstanding = 1m,
                ReplicationVersion = 1,
                CreatedAt = dayB,
                SourceLastKnownAt = dayB
            });
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);

        var rollup = await Db.ReportedOrderStatusByDays.AsNoTracking().OrderBy(r => r.DayUtc).ThenBy(r => r.OrderStatus).ToListAsync();

        Assert.That(rollup, Has.Count.EqualTo(2));
        var dayOnlyA = DateOnly.FromDateTime(dayA.UtcDateTime);
        var dayOnlyB = DateOnly.FromDateTime(dayB.UtcDateTime);

        Assert.Multiple(() =>
        {
            Assert.That(rollup[0].DayUtc, Is.EqualTo(dayOnlyA));
            Assert.That(rollup[0].OrderStatus, Is.EqualTo(OrderStatus.Pending));
            Assert.That(rollup[0].Count, Is.EqualTo(2));

            Assert.That(rollup[1].DayUtc, Is.EqualTo(dayOnlyB));
            Assert.That(rollup[1].OrderStatus, Is.EqualTo(OrderStatus.Processing));
            Assert.That(rollup[1].Count, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Second_refresh_replaces_previous_rollups()
    {
        var t = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        Db.ReportedOrders.Add(new ReportedOrder
        {
            Id = "x",
            OrderNumber = "x",
            CustomerId = "c",
            OrderStatus = OrderStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 1m,
            AmountPaid = 0m,
            AmountOutstanding = 1m,
            ReplicationVersion = 1,
            CreatedAt = t,
            SourceLastKnownAt = t
        });
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);
        Assert.That(await Db.ReportedOrderStatusByDays.CountAsync(), Is.EqualTo(1));

        Db.ReportedOrders.RemoveRange(Db.ReportedOrders);
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);
        Assert.That(await Db.ReportedOrderStatusByDays.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task Refresh_with_more_than_fifty_orders_aggregates_same_day_and_status()
    {
        const int orderCount = 51;
        var placedAt = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        for (var i = 0; i < orderCount; i++)
        {
            var id = $"o{i:D3}";
            Db.ReportedOrders.Add(new ReportedOrder
            {
                Id = id,
                OrderNumber = id,
                CustomerId = "c",
                OrderStatus = OrderStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 1m,
                AmountPaid = 0m,
                AmountOutstanding = 1m,
                ReplicationVersion = 1,
                CreatedAt = placedAt,
                SourceLastKnownAt = placedAt
            });
        }

        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);

        var dayOnly = DateOnly.FromDateTime(placedAt.UtcDateTime);
        var rollup = await Db.ReportedOrderStatusByDays.AsNoTracking()
            .SingleAsync(x => x.DayUtc == dayOnly && x.OrderStatus == OrderStatus.Pending);

        Assert.That(rollup.Count, Is.EqualTo(orderCount));
    }
}
