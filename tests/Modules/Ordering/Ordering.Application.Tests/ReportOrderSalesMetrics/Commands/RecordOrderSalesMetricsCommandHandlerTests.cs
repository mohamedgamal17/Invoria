using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.CompleteOrder;
using Invoria.Ordering.Application.ReportOrderSalesMetrics.Commands.RecordOrderSalesMetrics;
using Invoria.Ordering.Application.Tests.Orders;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Tests.ReportOrderSalesMetrics.Commands;

using ReportOrderSalesMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesMetrics;

[TestFixture]
public class RecordOrderSalesMetricsCommandHandlerTests : OrderTestFixture
{
    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearDataAsync();
    }

    private async Task ClearDataAsync()
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var reports = await db.Set<ReportOrderSalesMetricsEntity>().ToListAsync();
        db.RemoveRange(reports);
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_record_sales_metrics_for_every_period()
    {
        var order = await SeedCompletedOrderAsync();
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        var result = await Mediator.Send(new RecordOrderSalesMetricsCommand
        {
            OrderId = order.Id,
            OccurredOn = occurredOn
        });

        result.ShouldBeSuccess();

        var expected = await LoadContributionAsync(order.Id);

        var reports = await Scope.Resolve<OrderingDbContext>().Set<ReportOrderSalesMetricsEntity>().ToListAsync();

        reports.Should().HaveCount(4);

        AssertPeriod(reports, ReportPeriod.Daily,
            new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero), expected);
        AssertPeriod(reports, ReportPeriod.Monthly,
            new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero), expected);
        AssertPeriod(reports, ReportPeriod.Yearly,
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), expected);
        AssertPeriod(reports, ReportPeriod.AllTheTime, DateTimeOffset.MinValue, expected);
    }

    [Test]
    public async Task Should_aggregate_multiple_orders_in_same_period()
    {
        var order1 = await SeedCompletedOrderAsync();
        var order2 = await SeedCompletedOrderAsync();
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        await Mediator.Send(new RecordOrderSalesMetricsCommand
        {
            OrderId = order1.Id,
            OccurredOn = occurredOn
        });

        await Mediator.Send(new RecordOrderSalesMetricsCommand
        {
            OrderId = order2.Id,
            OccurredOn = occurredOn
        });

        var expected1 = await LoadContributionAsync(order1.Id);
        var expected2 = await LoadContributionAsync(order2.Id);
        var combined = new SalesContribution(
            expected1.TotalAmount + expected2.TotalAmount,
            expected1.TotalNetAmount + expected2.TotalNetAmount,
            expected1.TotalReturnAmount + expected2.TotalReturnAmount);

        var reports = await GetReportsAsync();

        reports.Should().HaveCount(4);

        AssertPeriod(reports, ReportPeriod.Daily,
            new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero), combined);
        AssertPeriod(reports, ReportPeriod.Monthly,
            new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero), combined);
        AssertPeriod(reports, ReportPeriod.Yearly,
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), combined);
        AssertPeriod(reports, ReportPeriod.AllTheTime, DateTimeOffset.MinValue, combined);
    }

    [Test]
    public async Task Should_include_return_amounts_net_of_returns()
    {
        var order = await SeedCompletedOrderAsync(true);
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        var result = await Mediator.Send(new RecordOrderSalesMetricsCommand
        {
            OrderId = order.Id,
            OccurredOn = occurredOn
        });

        result.ShouldBeSuccess();

        var expected = await LoadContributionAsync(order.Id);

        expected.TotalReturnAmount.Should().BeGreaterThan(0m);
        expected.TotalNetAmount.Should().Be(expected.TotalAmount - expected.TotalReturnAmount);

        var report = (await GetReportsAsync()).Single(x => x.Period == ReportPeriod.Daily);
        report.TotalAmount.Should().Be(expected.TotalAmount);
        report.TotalNetAmount.Should().Be(expected.TotalNetAmount);
        report.TotalReturnAmount.Should().Be(expected.TotalReturnAmount);
    }

    [Test]
    public async Task Should_bucket_by_occurred_on_into_distinct_period_rows()
    {
        var order1 = await SeedCompletedOrderAsync();
        var order2 = await SeedCompletedOrderAsync();
        var mayOccurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);
        var juneOccurredOn = new DateTimeOffset(2024, 6, 9, 14, 0, 0, TimeSpan.Zero);

        await Mediator.Send(new RecordOrderSalesMetricsCommand
        {
            OrderId = order1.Id,
            OccurredOn = mayOccurredOn
        });

        await Mediator.Send(new RecordOrderSalesMetricsCommand
        {
            OrderId = order2.Id,
            OccurredOn = juneOccurredOn
        });

        var expected1 = await LoadContributionAsync(order1.Id);
        var expected2 = await LoadContributionAsync(order2.Id);

        var reports = await GetReportsAsync();

        var daily = reports.Where(x => x.Period == ReportPeriod.Daily).ToList();
        daily.Should().HaveCount(2);
        AssertPeriodReport(daily.Single(x => x.Date == new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero)), expected1);
        AssertPeriodReport(daily.Single(x => x.Date == new DateTimeOffset(2024, 6, 9, 0, 0, 0, TimeSpan.Zero)), expected2);

        var monthly = reports.Where(x => x.Period == ReportPeriod.Monthly).ToList();
        monthly.Should().HaveCount(2);
        AssertPeriodReport(monthly.Single(x => x.Date == new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero)), expected1);
        AssertPeriodReport(monthly.Single(x => x.Date == new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)), expected2);

        var yearly = reports.Where(x => x.Period == ReportPeriod.Yearly).ToList();
        yearly.Should().HaveCount(1);
        AssertPeriodReport(yearly.Single(), new SalesContribution(
            expected1.TotalAmount + expected2.TotalAmount,
            expected1.TotalNetAmount + expected2.TotalNetAmount,
            expected1.TotalReturnAmount + expected2.TotalReturnAmount));

        var allTime = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTime.TotalAmount.Should().Be(expected1.TotalAmount + expected2.TotalAmount);
        allTime.TotalNetAmount.Should().Be(expected1.TotalNetAmount + expected2.TotalNetAmount);
        allTime.TotalReturnAmount.Should().Be(expected1.TotalReturnAmount + expected2.TotalReturnAmount);
    }

    private async Task<Order> SeedCompletedOrderAsync(bool withReturn = false)
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        if (withReturn)
        {
            var lineId = await Scope.Resolve<OrderingDbContext>()
                .Set<Order>()
                .Where(o => o.Id == order.Id)
                .SelectMany(o => o.Items)
                .Select(i => i.Id)
                .FirstAsync();

            await Mediator.Send(new CompleteOrderCommand(order.Id, [new CompleteReturnItemLine(lineId, 1)]));
        }
        else
        {
            await Mediator.Send(new CompleteOrderCommand(order.Id));
        }

        return order;
    }

    private async Task<SalesContribution> LoadContributionAsync(string orderId)
    {
        var db = Scope.Resolve<OrderingDbContext>();

        var order = await db.Set<Order>()
            .Include(o => o.Items)
            .Include(o => o.ReturnItems)
            .SingleAsync(o => o.Id == orderId);

        return new SalesContribution(
            order.TotalOrderAmount,
            order.NetOfTotalOrderAmount,
            order.TotalReturnAmount);
    }

    private async Task<List<ReportOrderSalesMetricsEntity>> GetReportsAsync()
    {
        return await Scope.Resolve<OrderingDbContext>()
            .Set<ReportOrderSalesMetricsEntity>()
            .ToListAsync();
    }

    private static void AssertPeriod(
        List<ReportOrderSalesMetricsEntity> reports,
        ReportPeriod period,
        DateTimeOffset expectedDate,
        SalesContribution expected)
    {
        var report = reports.Single(x => x.Period == period);

        report.Date.Should().Be(expectedDate);
        AssertPeriodReport(report, expected);
    }

    private static void AssertPeriodReport(ReportOrderSalesMetricsEntity report, SalesContribution expected)
    {
        report.TotalAmount.Should().Be(expected.TotalAmount);
        report.TotalNetAmount.Should().Be(expected.TotalNetAmount);
        report.TotalReturnAmount.Should().Be(expected.TotalReturnAmount);
    }

    private readonly record struct SalesContribution(
        decimal TotalAmount,
        decimal TotalNetAmount,
        decimal TotalReturnAmount);
}