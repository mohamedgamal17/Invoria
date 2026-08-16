using System.Reflection;
using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Commands.RecordOrderSalesProfitMetrics;
using Invoria.Ordering.Application.Tests.Orders;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Tests.ReportOrderSalesProfitMetrics.Commands;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

[TestFixture]
public class RecordOrderSalesProfitMetricsCommandHandlerTests : OrderTestFixture
{
    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearDataAsync();
    }

    private async Task ClearDataAsync()
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var batches = await db.Set<OrderAllocationConsumptionBatch>().ToListAsync();
        db.RemoveRange(batches);
        var lines = await db.Set<OrderAllocationConsumptionLine>().ToListAsync();
        db.RemoveRange(lines);
        var consumptions = await db.Set<OrderAllocationConsumption>().ToListAsync();
        db.RemoveRange(consumptions);
        var reports = await db.Set<ReportOrderSalesProfitMetricsEntity>().ToListAsync();
        db.RemoveRange(reports);
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_record_sales_profit_for_every_period()
    {
        var (order, itemId) = await SeedOrderAsync();
        var consumption = await SeedConsumptionAsync(order, itemId, "alloc-1");
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        var result = await Mediator.Send(new RecordOrderSalesProfitMetricsCommand
        {
            OrderId = order.Id,
            AllocationId = consumption.AllocationId,
            OccurredOn = occurredOn
        });

        result.ShouldBeSuccess();

        var expected = await LoadContributionAsync(order.Id, consumption.AllocationId);

        var reports = await GetReportsAsync();

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
        var (order1, itemId1) = await SeedOrderAsync();
        var (order2, itemId2) = await SeedOrderAsync();
        var consumption1 = await SeedConsumptionAsync(order1, itemId1, "alloc-1");
        var consumption2 = await SeedConsumptionAsync(order2, itemId2, "alloc-2");
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        await Mediator.Send(new RecordOrderSalesProfitMetricsCommand
        {
            OrderId = order1.Id,
            AllocationId = consumption1.AllocationId,
            OccurredOn = occurredOn
        });

        await Mediator.Send(new RecordOrderSalesProfitMetricsCommand
        {
            OrderId = order2.Id,
            AllocationId = consumption2.AllocationId,
            OccurredOn = occurredOn
        });

        var expected1 = await LoadContributionAsync(order1.Id, consumption1.AllocationId);
        var expected2 = await LoadContributionAsync(order2.Id, consumption2.AllocationId);
        var combined = new SalesProfitContribution(
            expected1.TotalRevenue + expected2.TotalRevenue,
            expected1.TotalCost + expected2.TotalCost,
            expected1.TotalProfit + expected2.TotalProfit,
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
    public async Task Should_exclude_returns_from_revenue_and_cost()
    {
        var (order, itemId) = await SeedOrderAsync(withReturn: true);
        var consumption = await SeedConsumptionAsync(order, itemId, "alloc-1");
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        var result = await Mediator.Send(new RecordOrderSalesProfitMetricsCommand
        {
            OrderId = order.Id,
            AllocationId = consumption.AllocationId,
            OccurredOn = occurredOn
        });

        result.ShouldBeSuccess();

        var expected = await LoadContributionAsync(order.Id, consumption.AllocationId);

        expected.TotalReturnAmount.Should().BeGreaterThan(0m);
        expected.TotalCost.Should().BeLessThan(40m);
        expected.TotalProfit.Should().Be(expected.TotalRevenue - expected.TotalCost);

        var report = (await GetReportsAsync()).Single(x => x.Period == ReportPeriod.Daily);
        report.TotalRevenue.Should().Be(expected.TotalRevenue);
        report.TotalCost.Should().Be(expected.TotalCost);
        report.TotalProfit.Should().Be(expected.TotalProfit);
        report.TotalReturnAmount.Should().Be(expected.TotalReturnAmount);
    }

    [Test]
    public async Task Should_fail_when_consumption_not_found()
    {
        var (order, _) = await SeedOrderAsync();
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        var result = await Mediator.Send(new RecordOrderSalesProfitMetricsCommand
        {
            OrderId = order.Id,
            AllocationId = "missing-allocation",
            OccurredOn = occurredOn
        });

        result.ShouldBeFailure(typeof(InvalidOperationException));
    }

    [Test]
    public async Task Should_bucket_by_occurred_on_into_distinct_period_rows()
    {
        var (order1, itemId1) = await SeedOrderAsync();
        var (order2, itemId2) = await SeedOrderAsync();
        var consumption1 = await SeedConsumptionAsync(order1, itemId1, "alloc-1");
        var consumption2 = await SeedConsumptionAsync(order2, itemId2, "alloc-2");
        var mayOccurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);
        var juneOccurredOn = new DateTimeOffset(2024, 6, 9, 14, 0, 0, TimeSpan.Zero);

        await Mediator.Send(new RecordOrderSalesProfitMetricsCommand
        {
            OrderId = order1.Id,
            AllocationId = consumption1.AllocationId,
            OccurredOn = mayOccurredOn
        });

        await Mediator.Send(new RecordOrderSalesProfitMetricsCommand
        {
            OrderId = order2.Id,
            AllocationId = consumption2.AllocationId,
            OccurredOn = juneOccurredOn
        });

        var expected1 = await LoadContributionAsync(order1.Id, consumption1.AllocationId);
        var expected2 = await LoadContributionAsync(order2.Id, consumption2.AllocationId);

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
        AssertPeriodReport(yearly.Single(), new SalesProfitContribution(
            expected1.TotalRevenue + expected2.TotalRevenue,
            expected1.TotalCost + expected2.TotalCost,
            expected1.TotalProfit + expected2.TotalProfit,
            expected1.TotalReturnAmount + expected2.TotalReturnAmount));

        var allTime = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTime.TotalRevenue.Should().Be(expected1.TotalRevenue + expected2.TotalRevenue);
        allTime.TotalCost.Should().Be(expected1.TotalCost + expected2.TotalCost);
        allTime.TotalProfit.Should().Be(expected1.TotalProfit + expected2.TotalProfit);
        allTime.TotalReturnAmount.Should().Be(expected1.TotalReturnAmount + expected2.TotalReturnAmount);
    }

    private async Task<(Order Order, string ItemId)> SeedOrderAsync(bool withReturn = false)
    {
        var order = new Order($"PROFIT-{Guid.NewGuid():N}", Guid.NewGuid().ToString());
        var itemId = Guid.NewGuid().ToString("N");
        var item = new OrderItem("product-1", 5, 20m);
        AssignStringEntityId(item, itemId);
        order.UpdateItems([item]);
        order.Accept();
        order.Complete(withReturn ? [new OrderReturnItem(itemId, 2)] : []);
        await OrderRepository.Add(order, CancellationToken.None);
        return (order, itemId);
    }

    private async Task<OrderAllocationConsumption> SeedConsumptionAsync(
        Order order,
        string orderItemId,
        string allocationId)
    {
        var consumptionRepository = Scope.Resolve<IOrderingRepository<OrderAllocationConsumption>>();

        var line = new OrderAllocationConsumptionLine(
            Guid.NewGuid().ToString("N"),
            orderItemId,
            "product-1",
            5);
        line.AddBatchAllocation(new OrderAllocationConsumptionBatch(
            Guid.NewGuid().ToString("N"),
            "batch-1",
            5,
            8m));

        var consumption = OrderAllocationConsumption.Create(order.Id, allocationId, [line]);
        consumption.ClearDomainEvents();
        await consumptionRepository.Add(consumption, CancellationToken.None);
        return consumption;
    }

    private async Task<SalesProfitContribution> LoadContributionAsync(string orderId, string allocationId)
    {
        var db = Scope.Resolve<OrderingDbContext>();

        var order = await db.Set<Order>()
            .Include(o => o.Items)
            .Include(o => o.ReturnItems)
            .SingleAsync(o => o.Id == orderId);

        var consumption = await db.Set<OrderAllocationConsumption>()
            .Include(c => c.Lines)
            .ThenInclude(l => l.BatchAllocations)
            .SingleAsync(c => c.AllocationId == allocationId);

        var returnedQuantities = order.ReturnItems
            .GroupBy(r => r.OrderItemId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

        var totalCost = consumption.Lines.Sum(line =>
        {
            var lineCost = line.BatchAllocations.Sum(b => b.UnitPrice * b.Quantity);
            var returnedQuantity = returnedQuantities.TryGetValue(line.OrderItemId, out var quantity)
                ? quantity
                : 0;
            var billableQuantity = Math.Max(0, line.QuantityRequested - returnedQuantity);
            return lineCost * billableQuantity / line.QuantityRequested;
        });

        var totalRevenue = order.NetOfTotalOrderAmount;

        return new SalesProfitContribution(
            totalRevenue,
            totalCost,
            totalRevenue - totalCost,
            order.TotalReturnAmount);
    }

    private async Task<List<ReportOrderSalesProfitMetricsEntity>> GetReportsAsync()
    {
        return await Scope.Resolve<OrderingDbContext>()
            .Set<ReportOrderSalesProfitMetricsEntity>()
            .ToListAsync();
    }

    private static void AssignStringEntityId(Entity<string> entity, string id)
    {
        var property = typeof(Entity<string>).GetProperty(
            nameof(Entity<string>.Id),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        property.SetValue(entity, id);
    }

    private static void AssertPeriod(
        List<ReportOrderSalesProfitMetricsEntity> reports,
        ReportPeriod period,
        DateTimeOffset expectedDate,
        SalesProfitContribution expected)
    {
        var report = reports.Single(x => x.Period == period);

        report.Date.Should().Be(expectedDate);
        AssertPeriodReport(report, expected);
    }

    private static void AssertPeriodReport(ReportOrderSalesProfitMetricsEntity report, SalesProfitContribution expected)
    {
        report.TotalRevenue.Should().Be(expected.TotalRevenue);
        report.TotalCost.Should().Be(expected.TotalCost);
        report.TotalProfit.Should().Be(expected.TotalProfit);
        report.TotalReturnAmount.Should().Be(expected.TotalReturnAmount);
    }

    private readonly record struct SalesProfitContribution(
        decimal TotalRevenue,
        decimal TotalCost,
        decimal TotalProfit,
        decimal TotalReturnAmount);
}
