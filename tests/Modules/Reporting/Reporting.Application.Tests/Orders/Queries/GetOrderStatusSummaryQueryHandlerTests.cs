using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Reporting.Application.Orders.Queries.GetOrderStatusSummary;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.StatusSummary;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Invoria.Reporting.Domain.Repositories;
using Invoria.Reporting.Infrastructure.EntityFramework.Repositories;
using Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;
using Microsoft.Extensions.Logging.Abstractions;

namespace Invoria.Reporting.Application.Tests.Orders.Queries;

[TestFixture]
public sealed class GetOrderStatusSummaryQueryHandlerTests
{
    private static ReportedOrder CreateOrder(string id, OrderStatus status, DateTimeOffset createdAt)
    {
        return new ReportedOrder
        {
            Id = id,
            OrderNumber = id,
            CustomerId = "c1",
            OrderStatus = status,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 1m,
            AmountPaid = 0m,
            AmountOutstanding = 1m,
            ReplicationVersion = 1,
            SourceLastKnownAt = createdAt,
            CreatedAt = createdAt
        };
    }

    [Test]
    public async Task After_refresh_returns_counts_for_date_range_summed_by_status()
    {
        var may1 = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var may2 = DateTimeOffset.Parse("2026-05-02T08:00:00Z");

        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        db.ReportedOrders.AddRange(
            CreateOrder("o1", OrderStatus.Pending, may1),
            CreateOrder("o2", OrderStatus.Pending, may1),
            CreateOrder("o3", OrderStatus.Completed, may2));
        await db.SaveChangesAsync();

        var refresher = new ReportedOrderStatusSummaryRollupRefresher(db, NullLogger<ReportedOrderStatusSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var repository = new ReportingRepository<ReportedOrderStatusByDay>(db);
        var handler = new GetOrderStatusSummaryQueryHandler(repository);

        var from = DateTimeOffset.Parse("2026-05-01T00:00:00Z");
        var to = DateTimeOffset.Parse("2026-05-02T23:59:59Z");
        var result = await handler.Handle(
            new GetOrderStatusSummaryQuery { FromUtc = from, ToUtc = to },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        var rows = result.Value!.ToList();
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(rows[0].Status, Is.EqualTo(OrderStatus.Pending));
            Assert.That(rows[0].Count, Is.EqualTo(2));
            Assert.That(rows[1].Status, Is.EqualTo(OrderStatus.Completed));
            Assert.That(rows[1].Count, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task After_refresh_excludes_orders_outside_requested_day_range()
    {
        var may1 = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var may3 = DateTimeOffset.Parse("2026-05-03T10:00:00Z");

        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        db.ReportedOrders.AddRange(
            CreateOrder("o1", OrderStatus.Pending, may1),
            CreateOrder("o2", OrderStatus.Completed, may3));
        await db.SaveChangesAsync();

        var refresher = new ReportedOrderStatusSummaryRollupRefresher(db, NullLogger<ReportedOrderStatusSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var repository = new ReportingRepository<ReportedOrderStatusByDay>(db);
        var handler = new GetOrderStatusSummaryQueryHandler(repository);

        var from = DateTimeOffset.Parse("2026-05-01T00:00:00Z");
        var to = DateTimeOffset.Parse("2026-05-01T23:59:59Z");
        var result = await handler.Handle(
            new GetOrderStatusSummaryQuery { FromUtc = from, ToUtc = to },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var rows = result.Value!.ToList();
        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(rows[0].Status, Is.EqualTo(OrderStatus.Pending));
            Assert.That(rows[0].Count, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Returns_empty_when_rollout_has_no_rows_in_range()
    {
        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();

        var refresher = new ReportedOrderStatusSummaryRollupRefresher(db, NullLogger<ReportedOrderStatusSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var repository = new ReportingRepository<ReportedOrderStatusByDay>(db);
        var handler = new GetOrderStatusSummaryQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrderStatusSummaryQuery
            {
                FromUtc = DateTimeOffset.Parse("2026-05-01T00:00:00Z"),
                ToUtc = DateTimeOffset.Parse("2026-05-01T23:59:59Z")
            },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value!, Is.Empty);
    }

    [Test]
    public async Task After_refresh_without_time_frame_returns_all_rollups_summed_by_status()
    {
        var may1 = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var may2 = DateTimeOffset.Parse("2026-05-02T08:00:00Z");

        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        db.ReportedOrders.AddRange(
            CreateOrder("o1", OrderStatus.Pending, may1),
            CreateOrder("o2", OrderStatus.Pending, may1),
            CreateOrder("o3", OrderStatus.Completed, may2));
        await db.SaveChangesAsync();

        var refresher = new ReportedOrderStatusSummaryRollupRefresher(db, NullLogger<ReportedOrderStatusSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var repository = new ReportingRepository<ReportedOrderStatusByDay>(db);
        var handler = new GetOrderStatusSummaryQueryHandler(repository);

        var result = await handler.Handle(new GetOrderStatusSummaryQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var rows = result.Value!.ToList();
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(rows[0].Status, Is.EqualTo(OrderStatus.Pending));
            Assert.That(rows[0].Count, Is.EqualTo(2));
            Assert.That(rows[1].Status, Is.EqualTo(OrderStatus.Completed));
            Assert.That(rows[1].Count, Is.EqualTo(1));
        });
    }
}
