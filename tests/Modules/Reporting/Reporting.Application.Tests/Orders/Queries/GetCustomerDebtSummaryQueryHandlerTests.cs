using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Application.Orders.Queries.GetCustomerDebtSummary;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.DebtSummary;
using Invoria.Reporting.Domain.Repositories;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Invoria.Reporting.Infrastructure.EntityFramework.Repositories;
using Invoria.Reporting.Infrastructure.Orders.Materialization.DebtSummary;
using Microsoft.Extensions.Logging.Abstractions;

namespace Invoria.Reporting.Application.Tests.Orders.Queries;

[TestFixture]
public sealed class GetCustomerDebtSummaryQueryHandlerTests
{
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
    public async Task Handle_returns_customer_debt_overview_after_refresh()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        db.ReportedOrders.AddRange(
            CreateOrder("o1", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 100m, 0m, t),
            CreateOrder("o2", "c1", OrderStatus.Completed, OrderPaymentStatus.Partial, 50m, 20m, t),
            CreateOrder("o3", "c2", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 40m, 0m, t));
        await db.SaveChangesAsync();

        var refresher = new DebtSummaryRollupRefresher(db, NullLogger<DebtSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var handler = new GetCustomerDebtSummaryQueryHandler(new ReportingRepository<DebtSummaryBase>(db));

        var result = await handler.Handle(
            new GetCustomerDebtSummaryQuery { CustomerId = "c1" },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var dto = result.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(dto.CustomerId, Is.EqualTo("c1"));
            Assert.That(dto.DebtOrderCount, Is.EqualTo(2));
            Assert.That(dto.TotalOrderValue, Is.EqualTo(150m));
            Assert.That(dto.TotalPaid, Is.EqualTo(20m));
            Assert.That(dto.TotalOutstanding, Is.EqualTo(130m));
            Assert.That(dto.UnpaidCount, Is.EqualTo(1));
            Assert.That(dto.PartiallyPaidCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Handle_returns_not_found_when_customer_has_no_debt_row()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        await using var db = await ReportingQueryTestDbContextFactory.CreateAsync();
        db.ReportedOrders.Add(
            CreateOrder("o1", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 40m, 0m, t));
        await db.SaveChangesAsync();

        var refresher = new DebtSummaryRollupRefresher(db, NullLogger<DebtSummaryRollupRefresher>.Instance);
        await refresher.RefreshAsync(CancellationToken.None);

        var handler = new GetCustomerDebtSummaryQueryHandler(new ReportingRepository<DebtSummaryBase>(db));

        var result = await handler.Handle(
            new GetCustomerDebtSummaryQuery { CustomerId = "unknown" },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception, Is.InstanceOf<NotFoundException>());
    }
}
