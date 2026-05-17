using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Application.Orders.Queries.ListOrderPeriodSummary;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Invoria.Reporting.Infrastructure.Orders.Materialization.OrderPeriodSummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Invoria.Reporting.Application.Tests.Orders.Queries;

[TestFixture]
public sealed class ListOrderPeriodSummaryQueryHandlerTests
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

    private static async Task<ListOrderPeriodSummaryQueryHandler> CreateHandlerWithRollupAsync(ReportingDbContext db)
    {
        var refresher = new OrderPeriodSummaryRollupRefresher(
            db,
            NullLogger<OrderPeriodSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var reader = new OrderPeriodSummaryRollupReader(db);
        return new ListOrderPeriodSummaryQueryHandler(reader);
    }

    private static ReportedOrder CreateOrder(
        string id,
        DateTimeOffset createdAt,
        decimal total,
        OrderStatus status,
        List<ReportedOrderStateTransition>? transitions = null) =>
        new()
        {
            Id = id,
            OrderNumber = id,
            CustomerId = "c1",
            OrderStatus = status,
            FullfillmentStatus = FullfillmentStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = total,
            AmountPaid = 0m,
            AmountOutstanding = total,
            ReplicationVersion = 1,
            SourceLastKnownAt = createdAt,
            CreatedAt = createdAt,
            Lines = new List<ReportedOrderLine>(),
            StateTransitions = transitions ?? new List<ReportedOrderStateTransition>()
        };

    [Test]
    public async Task Placed_day_range_returns_overlapping_day_bucket_only()
    {
        var may1 = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var may3 = DateTimeOffset.Parse("2026-05-03T10:00:00Z");

        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        db.ReportedOrders.AddRange(
            CreateOrder("o1", may1, 10m, OrderStatus.Pending),
            CreateOrder("o2", may3, 20m, OrderStatus.Pending));
        await db.SaveChangesAsync();

        var handler = await CreateHandlerWithRollupAsync(db);

        var result = await handler.Handle(
            new ListOrderPeriodSummaryQuery
            {
                Skip = 0,
                Length = 50,
                From = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                To = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc),
                GroupedBy = OrderPeriodSummaryGranularity.Day
            },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var page = result.Value!;
        Assert.That(page.Info.TotalCount, Is.EqualTo(1));
        var row = page.Data.Single();
        Assert.That(row.PeriodKey, Is.EqualTo("2026-05-01"));
        Assert.That(row.OrderCount, Is.EqualTo(1));
        Assert.That(row.GrossRevenue, Is.EqualTo(10m));
    }

    [Test]
    public async Task Week_bucket_label_uses_monday_of_week_in_utc()
    {
        var wed = DateTimeOffset.Parse("2026-05-06T12:00:00Z");

        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        db.ReportedOrders.Add(CreateOrder("o1", wed, 1m, OrderStatus.Pending));
        await db.SaveChangesAsync();

        var handler = await CreateHandlerWithRollupAsync(db);

        var result = await handler.Handle(
            new ListOrderPeriodSummaryQuery
            {
                Skip = 0,
                Length = 50,
                From = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                To = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc),
                GroupedBy = OrderPeriodSummaryGranularity.Week
            },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var week = result.Value!.Data.Single().PeriodKey;
        Assert.That(week, Is.EqualTo("W/c 2026-05-04"));
    }

    [Test]
    public async Task Pagination_applies_to_period_rows()
    {
        var may1 = DateTimeOffset.Parse("2026-05-01T10:00:00Z");
        var may2 = DateTimeOffset.Parse("2026-05-02T10:00:00Z");
        var may3 = DateTimeOffset.Parse("2026-05-03T10:00:00Z");

        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        db.ReportedOrders.AddRange(
            CreateOrder("a", may1, 1m, OrderStatus.Pending),
            CreateOrder("b", may2, 2m, OrderStatus.Pending),
            CreateOrder("c", may3, 3m, OrderStatus.Pending));
        await db.SaveChangesAsync();

        var handler = await CreateHandlerWithRollupAsync(db);

        var result = await handler.Handle(
            new ListOrderPeriodSummaryQuery
            {
                Skip = 1,
                Length = 1,
                From = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                To = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc),
                GroupedBy = OrderPeriodSummaryGranularity.Day
            },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var page = result.Value!;
        Assert.That(page.Info.TotalCount, Is.EqualTo(3));
        Assert.That(page.Data.Count(), Is.EqualTo(1));
        Assert.That(page.Data.Single().PeriodKey, Is.EqualTo("2026-05-02"));
    }
}
