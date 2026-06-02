using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.OrderPeriodSummary;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Tests.Orders.Materialization.OrderPeriodSummary;

[TestFixture]
public sealed class OrderPeriodSummaryRollupRefresherTests : OrderPeriodSummaryRollupRefresherTestFixture
{
    [Test]
    public async Task Refresh_materializes_day_week_and_month_for_placed_date_field()
    {
        var placedAt = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        Db.ReportedOrders.Add(new ReportedOrder
        {
            Id = "o1",
            OrderNumber = "o1",
            CustomerId = "c",
            OrderStatus = OrderStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 10m,
            AmountPaid = 0m,
            AmountOutstanding = 10m,
            ReplicationVersion = 1,
            CreatedAt = placedAt,
            SourceLastKnownAt = placedAt
        });
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);

        const int placed = 0;
        var day = await Db.OrderPeriodSummaries.AsNoTracking()
            .SingleAsync(x => x.Granularity == nameof(OrderPeriodSummaryGranularity.Day)
                               && x.DateField == placed
                               && x.PeriodKey == "2026-05-01");
        var week = await Db.OrderPeriodSummaries.AsNoTracking()
            .SingleAsync(x => x.Granularity == nameof(OrderPeriodSummaryGranularity.Week)
                              && x.DateField == placed
                              && x.PeriodKey == "W/c 2026-04-27");
        var month = await Db.OrderPeriodSummaries.AsNoTracking()
            .SingleAsync(x => x.Granularity == nameof(OrderPeriodSummaryGranularity.Month)
                              && x.DateField == placed
                              && x.PeriodKey == "2026-05");

        Assert.Multiple(() =>
        {
            Assert.That(day.OrderCount, Is.EqualTo(1));
            Assert.That(day.GrossRevenue, Is.EqualTo(10m));

            Assert.That(week.OrderCount, Is.EqualTo(1));
            Assert.That(month.OrderCount, Is.EqualTo(1));
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
        Assert.That(await Db.OrderPeriodSummaries.CountAsync(), Is.GreaterThan(0));

        Db.ReportedOrders.RemoveRange(Db.ReportedOrders);
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);
        Assert.That(await Db.OrderPeriodSummaries.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task Refresh_with_more_than_fifty_orders_aggregates_day_bucket()
    {
        const int orderCount = 51;
        const decimal amountPerOrder = 2m;
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
                TotalOrderAmount = amountPerOrder,
                AmountPaid = 1m,
                AmountOutstanding = 1m,
                ReplicationVersion = 1,
                CreatedAt = placedAt,
                SourceLastKnownAt = placedAt
            });
        }

        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);

        const int placed = 0;
        var day = await Db.OrderPeriodSummaries.AsNoTracking()
            .SingleAsync(x => x.Granularity == nameof(OrderPeriodSummaryGranularity.Day)
                               && x.DateField == placed
                               && x.PeriodKey == "2026-05-01");

        Assert.Multiple(() =>
        {
            Assert.That(day.OrderCount, Is.EqualTo(orderCount));
            Assert.That(day.GrossRevenue, Is.EqualTo(orderCount * amountPerOrder));
            Assert.That(day.NetRevenue, Is.EqualTo(orderCount * 1m));
        });
    }

    [Test]
    public async Task Refresh_counts_cancelled_and_delivered()
    {
        var placedAt = DateTimeOffset.Parse("2026-05-02T10:00:00Z");

        Db.ReportedOrders.Add(new ReportedOrder
        {
            Id = "cancelled",
            OrderNumber = "cancelled",
            CustomerId = "c",
            OrderStatus = OrderStatus.Cancelled,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 5m,
            AmountPaid = 0m,
            AmountOutstanding = 5m,
            ReplicationVersion = 1,
            CreatedAt = placedAt,
            SourceLastKnownAt = placedAt
        });
        Db.ReportedOrders.Add(new ReportedOrder
        {
            Id = "completed",
            OrderNumber = "completed",
            CustomerId = "c",
            OrderStatus = OrderStatus.Completed,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Paid,
            TotalOrderAmount = 8m,
            AmountPaid = 8m,
            AmountOutstanding = 0m,
            ReplicationVersion = 1,
            CreatedAt = placedAt,
            SourceLastKnownAt = placedAt
        });
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);

        const int placed = 0;
        var day = await Db.OrderPeriodSummaries.AsNoTracking()
            .SingleAsync(x => x.Granularity == nameof(OrderPeriodSummaryGranularity.Day)
                               && x.DateField == placed
                               && x.PeriodKey == "2026-05-02");

        Assert.Multiple(() =>
        {
            Assert.That(day.OrderCount, Is.EqualTo(2));
            Assert.That(day.CancelledCount, Is.EqualTo(1));
            Assert.That(day.DeliveredCount, Is.EqualTo(1));
        });
    }
}
