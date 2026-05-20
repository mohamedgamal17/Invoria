using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Application.Orders.Queries.GetDebtOverview;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.DebtSummary;
using Invoria.Reporting.Domain.Repositories;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Invoria.Reporting.Infrastructure.EntityFramework.Repositories;
using Invoria.Reporting.Infrastructure.Orders.Materialization.DebtSummary;
using Microsoft.Extensions.Logging.Abstractions;

namespace Invoria.Reporting.Application.Tests.Orders.Queries;

[TestFixture]
public sealed class GetDebtOverviewQueryHandlerTests
{
    private static async Task<GetDebtOverviewQueryHandler> CreateHandlerWithDebtRollupAsync(
        ReportingDbContext db)
    {
        var refresher = new DebtSummaryRollupRefresher(
            db,
            NullLogger<DebtSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var repository = new ReportingRepository<DebtSummaryBase>(db);
        return new GetDebtOverviewQueryHandler(repository);
    }

    private static ReportedOrder CreateOrder(
        string id,
        string customerId,
        OrderStatus orderStatus,
        OrderPaymentStatus paymentStatus,
        decimal total,
        decimal amountPaid,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = id,
            OrderNumber = id,
            CustomerId = customerId,
            OrderStatus = orderStatus,
            FullfillmentStatus = FullfillmentStatus.Pending,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = paymentStatus,
            TotalOrderAmount = total,
            AmountPaid = amountPaid,
            AmountOutstanding = total - amountPaid,
            ReplicationVersion = 1,
            CreatedAt = createdAt,
            SourceLastKnownAt = createdAt
        };

    [Test]
    public async Task Handle_without_materialization_returns_zeroed_defaults()
    {
        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        var handler = new GetDebtOverviewQueryHandler(new ReportingRepository<DebtSummaryBase>(db));

        var result = await handler.Handle(new GetDebtOverviewQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var dto = result.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(dto.TotalOutstanding, Is.Zero);
            Assert.That(dto.TotalPaid, Is.Zero);
            Assert.That(dto.TotalOrderValue, Is.Zero);
            Assert.That(dto.DebtOrderCount, Is.Zero);
            Assert.That(dto.PartiallyPaidCount, Is.Zero);
            Assert.That(dto.UnpaidCount, Is.Zero);
            Assert.That(dto.CollectionRate, Is.Zero);
            Assert.That(dto.ComputedAt, Is.EqualTo(default(DateTimeOffset)));
        });
    }

    [Test]
    public async Task Handle_after_refresh_returns_global_debt_overview()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        db.ReportedOrders.AddRange(
            CreateOrder("o1", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 100m, 0m, t),
            CreateOrder("o2", "c1", OrderStatus.Completed, OrderPaymentStatus.Partial, 50m, 20m, t),
            CreateOrder("o3", "c2", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 40m, 0m, t));
        await db.SaveChangesAsync();

        var handler = await CreateHandlerWithDebtRollupAsync(db);

        var result = await handler.Handle(new GetDebtOverviewQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var dto = result.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(dto.DebtOrderCount, Is.EqualTo(3));
            Assert.That(dto.TotalOrderValue, Is.EqualTo(190m));
            Assert.That(dto.TotalPaid, Is.EqualTo(20m));
            Assert.That(dto.TotalOutstanding, Is.EqualTo(170m));
            Assert.That(dto.UnpaidCount, Is.EqualTo(2));
            Assert.That(dto.PartiallyPaidCount, Is.EqualTo(1));
            Assert.That(dto.CollectionRate, Is.EqualTo(10.53m));
            Assert.That(dto.ComputedAt, Is.Not.EqualTo(default));
        });
    }
}
