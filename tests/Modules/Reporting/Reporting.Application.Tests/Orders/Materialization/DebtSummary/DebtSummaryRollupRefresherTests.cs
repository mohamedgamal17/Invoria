using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.DebtSummary;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Tests.Orders.Materialization.DebtSummary;

[TestFixture]
public sealed class DebtSummaryRollupRefresherTests : DebtSummaryRollupRefresherTestFixture
{
    private static ReportedOrder CreateOrder(
        string id,
        string customerId,
        OrderStatus orderStatus,
        OrderPaymentStatus paymentStatus,
        OrderPaymentType paymentType,
        decimal totalOrderAmount,
        decimal amountPaid,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = id,
            OrderNumber = id,
            CustomerId = customerId,
            OrderStatus = orderStatus,
            PaymentType = paymentType,
            PaymentStatus = paymentStatus,
            TotalOrderAmount = totalOrderAmount,
            AmountPaid = amountPaid,
            AmountOutstanding = totalOrderAmount - amountPaid,
            ReplicationVersion = 1,
            CreatedAt = createdAt,
            SourceLastKnownAt = createdAt
        };

    [Test]
    public async Task Refresh_materializes_global_and_customer_summaries_for_completed_outstanding_orders()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateOrder("o1", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, OrderPaymentType.Debt, 100m, 0m, t),
            CreateOrder("o2", "c1", OrderStatus.Completed, OrderPaymentStatus.Partial, OrderPaymentType.Debt, 50m, 20m, t),
            CreateOrder("o3", "c2", OrderStatus.Completed, OrderPaymentStatus.Unpaid, OrderPaymentType.Immediate, 40m, 0m, t),
            CreateOrder("pending", "c1", OrderStatus.Pending, OrderPaymentStatus.Unpaid, OrderPaymentType.Debt, 200m, 0m, t),
            CreateOrder("paid", "c2", OrderStatus.Completed, OrderPaymentStatus.Paid, OrderPaymentType.Debt, 80m, 80m, t),
            CreateOrder("no-outstanding", "c2", OrderStatus.Completed, OrderPaymentStatus.Unpaid, OrderPaymentType.Debt, 10m, 10m, t));
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);

        var global = await Db.DebtSummaries
            .OfType<DebtGlobalSummary>()
            .AsNoTracking()
            .SingleAsync();

        var customers = await Db.DebtSummaries
            .OfType<DebtCustomerSummary>()
            .AsNoTracking()
            .OrderBy(x => x.CustomerId)
            .ToListAsync();

        Assert.That(customers, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(global.SummaryType, Is.EqualTo(DebtSummaryType.Global));
            Assert.That(global.DebtOrderCount, Is.EqualTo(3));
            Assert.That(global.TotalOrderValue, Is.EqualTo(190m));
            Assert.That(global.TotalPaid, Is.EqualTo(20m));
            Assert.That(global.TotalOutstanding, Is.EqualTo(170m));
            Assert.That(global.UnpaidCount, Is.EqualTo(2));
            Assert.That(global.PartiallyPaidCount, Is.EqualTo(1));
            Assert.That(global.CollectionRate, Is.EqualTo(10.53m));

            var c1 = customers.Single(x => x.CustomerId == "c1");
            Assert.That(c1.Id, Is.EqualTo("c1"));
            Assert.That(c1.SummaryType, Is.EqualTo(DebtSummaryType.Customer));
            Assert.That(c1.DebtOrderCount, Is.EqualTo(2));
            Assert.That(c1.TotalOrderValue, Is.EqualTo(150m));
            Assert.That(c1.TotalOutstanding, Is.EqualTo(130m));

            var c2 = customers.Single(x => x.CustomerId == "c2");
            Assert.That(c2.DebtOrderCount, Is.EqualTo(1));
            Assert.That(c2.TotalOutstanding, Is.EqualTo(40m));
        });
    }

    [Test]
    public async Task Refresh_selects_oldest_outstanding_debt_per_customer()
    {
        var older = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var newer = DateTimeOffset.Parse("2026-06-01T00:00:00Z");
        var sameInstant = DateTimeOffset.Parse("2026-03-01T00:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateOrder("old", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, OrderPaymentType.Debt, 10m, 0m, older),
            CreateOrder("new", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, OrderPaymentType.Debt, 99m, 0m, newer),
            CreateOrder("tie-low", "c2", OrderStatus.Completed, OrderPaymentStatus.Partial, OrderPaymentType.Debt, 5m, 1m, sameInstant),
            CreateOrder("tie-high", "c2", OrderStatus.Completed, OrderPaymentStatus.Partial, OrderPaymentType.Debt, 20m, 5m, sameInstant),
            CreateOrder("paid", "c2", OrderStatus.Completed, OrderPaymentStatus.Paid, OrderPaymentType.Debt, 1m, 1m, older));
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);

        var c1 = await Db.DebtSummaries
            .OfType<DebtCustomerSummary>()
            .AsNoTracking()
            .SingleAsync(x => x.CustomerId == "c1");

        var c2 = await Db.DebtSummaries
            .OfType<DebtCustomerSummary>()
            .AsNoTracking()
            .SingleAsync(x => x.CustomerId == "c2");

        Assert.Multiple(() =>
        {
            Assert.That(c1.OldestDebtDate, Is.EqualTo(older));
            Assert.That(c1.OldestDebtAmount, Is.EqualTo(10m));

            Assert.That(c2.OldestDebtDate, Is.EqualTo(sameInstant));
            Assert.That(c2.OldestDebtAmount, Is.EqualTo(15m));
        });
    }

    [Test]
    public async Task Refresh_with_more_than_fifty_debt_orders_aggregates_global()
    {
        const int orderCount = 51;
        const decimal amountPerOrder = 2m;
        var placedAt = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        for (var i = 0; i < orderCount; i++)
        {
            var id = $"o{i:D3}";
            Db.ReportedOrders.Add(CreateOrder(
                id,
                "c",
                OrderStatus.Completed,
                OrderPaymentStatus.Unpaid,
                OrderPaymentType.Debt,
                amountPerOrder,
                1m,
                placedAt));
        }

        await Db.SaveChangesAsync();
        await Refresher.RefreshAsync(CancellationToken.None);

        var global = await Db.DebtSummaries
            .OfType<DebtGlobalSummary>()
            .AsNoTracking()
            .SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(global.DebtOrderCount, Is.EqualTo(orderCount));
            Assert.That(global.TotalOrderValue, Is.EqualTo(orderCount * amountPerOrder));
            Assert.That(global.TotalPaid, Is.EqualTo(orderCount * 1m));
            Assert.That(global.TotalOutstanding, Is.EqualTo(orderCount * 1m));
        });
    }

    [Test]
    public async Task Second_refresh_replaces_previous_debt_summaries()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        Db.ReportedOrders.Add(CreateOrder(
            "x",
            "c",
            OrderStatus.Completed,
            OrderPaymentStatus.Unpaid,
            OrderPaymentType.Debt,
            1m,
            0m,
            t));
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);
        Assert.That(await Db.DebtSummaries.CountAsync(), Is.GreaterThan(1));

        Db.ReportedOrders.RemoveRange(Db.ReportedOrders);
        await Db.SaveChangesAsync();

        await Refresher.RefreshAsync(CancellationToken.None);

        var summaries = await Db.DebtSummaries.AsNoTracking().ToListAsync();
        Assert.That(summaries, Has.Count.EqualTo(1));

        var global = summaries.OfType<DebtGlobalSummary>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(global.DebtOrderCount, Is.EqualTo(0));
            Assert.That(global.TotalOrderValue, Is.EqualTo(0m));
        });
    }
}
