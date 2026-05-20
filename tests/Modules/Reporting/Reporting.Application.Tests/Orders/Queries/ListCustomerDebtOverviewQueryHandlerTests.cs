using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Application.Orders.Queries.ListCustomerDebtOverview;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.DebtSummary;
using Invoria.Reporting.Domain.Repositories;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Invoria.Reporting.Infrastructure.EntityFramework.Repositories;
using Invoria.Reporting.Infrastructure.Orders.Materialization.DebtSummary;
using Microsoft.Extensions.Logging.Abstractions;

namespace Invoria.Reporting.Application.Tests.Orders.Queries;

[TestFixture]
public sealed class ListCustomerDebtOverviewQueryHandlerTests
{
    private static async Task<ListCustomerDebtOverviewQueryHandler> CreateHandlerWithDebtRollupAsync(
        ReportingDbContext db)
    {
        var refresher = new DebtSummaryRollupRefresher(
            db,
            NullLogger<DebtSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var repository = new ReportingRepository<DebtSummaryBase>(db);
        return new ListCustomerDebtOverviewQueryHandler(repository);
    }

    private static ReportedOrder CreateOrder(
        string id,
        string customerId,
        decimal total,
        decimal amountPaid,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = id,
            OrderNumber = id,
            CustomerId = customerId,
            OrderStatus = OrderStatus.Completed,
            FullfillmentStatus = FullfillmentStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = total,
            AmountPaid = amountPaid,
            AmountOutstanding = total - amountPaid,
            ReplicationVersion = 1,
            CreatedAt = createdAt,
            SourceLastKnownAt = createdAt
        };

    [Test]
    public async Task Handle_returns_customers_ordered_by_total_outstanding_desc()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        db.ReportedOrders.AddRange(
            CreateOrder("o1", "c-low", 10m, 0m, t),
            CreateOrder("o2", "c-high", 200m, 0m, t),
            CreateOrder("o3", "c-mid", 50m, 0m, t));
        await db.SaveChangesAsync();

        var handler = await CreateHandlerWithDebtRollupAsync(db);

        var result = await handler.Handle(
            new ListCustomerDebtOverviewQuery { Skip = 0, Length = 10 },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var page = result.Value!;
        Assert.That(page.Info.TotalCount, Is.EqualTo(3));
        var rows = page.Data.ToList();
        Assert.That(rows, Has.Count.EqualTo(3));
        Assert.That(rows[0].CustomerId, Is.EqualTo("c-high"));
        Assert.That(rows[0].TotalOutstanding, Is.EqualTo(200m));
        Assert.That(rows[1].CustomerId, Is.EqualTo("c-mid"));
        Assert.That(rows[2].CustomerId, Is.EqualTo("c-low"));
    }

    [Test]
    public async Task Handle_applies_skip_and_length()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        db.ReportedOrders.AddRange(
            CreateOrder("o1", "c1", 100m, 0m, t),
            CreateOrder("o2", "c2", 90m, 0m, t),
            CreateOrder("o3", "c3", 80m, 0m, t),
            CreateOrder("o4", "c4", 70m, 0m, t));
        await db.SaveChangesAsync();

        var handler = await CreateHandlerWithDebtRollupAsync(db);

        var result = await handler.Handle(
            new ListCustomerDebtOverviewQuery { Skip = 1, Length = 2 },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var page = result.Value!;
        var rows = page.Data.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(page.Info.TotalCount, Is.EqualTo(4));
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows[0].CustomerId, Is.EqualTo("c2"));
            Assert.That(rows[1].CustomerId, Is.EqualTo("c3"));
        });
    }
}
