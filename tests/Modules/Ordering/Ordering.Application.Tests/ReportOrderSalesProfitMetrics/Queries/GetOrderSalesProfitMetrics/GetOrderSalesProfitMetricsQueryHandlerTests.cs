using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Queries.GetOrderSalesProfitMetrics;
using Invoria.Ordering.Application.Tests.Orders;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Tests.ReportOrderSalesProfitMetrics.Queries.GetOrderSalesProfitMetrics;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

[TestFixture]
public class GetOrderSalesProfitMetricsQueryHandlerTests : OrderTestFixture
{
    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearDataAsync();
    }

    private async Task ClearDataAsync()
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var reports = await db.Set<ReportOrderSalesProfitMetricsEntity>().ToListAsync();
        db.RemoveRange(reports);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_return_zeroed_metrics_when_no_rows_exist()
    {
        var result = await Mediator.Send(new GetOrderSalesProfitMetricsQuery());

        result.ShouldBeSuccess();

        var now = DateTimeOffset.Now;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        result.Value!.ThisDay.Date.Should().Be(todayStart);
        result.Value!.ThisDay.TotalRevenue.Should().Be(0m);
        result.Value!.ThisDay.TotalCost.Should().Be(0m);
        result.Value!.ThisDay.TotalProfit.Should().Be(0m);
        result.Value!.ThisDay.TotalReturnAmount.Should().Be(0m);
        result.Value!.ThisDay.Period.Should().Be(ReportPeriod.Daily);

        result.Value!.ThisMonth.Date.Should().Be(monthStart);
        result.Value!.ThisMonth.Period.Should().Be(ReportPeriod.Monthly);

        result.Value!.ThisYear.Date.Should().Be(yearStart);
        result.Value!.ThisYear.Period.Should().Be(ReportPeriod.Yearly);

        result.Value!.AllTime.Date.Should().Be(DateTimeOffset.MinValue);
        result.Value!.AllTime.Period.Should().Be(ReportPeriod.AllTheTime);
    }

    [Test]
    public async Task Should_return_metrics_for_each_period_when_seeded()
    {
        var now = DateTimeOffset.Now;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        var repository = Scope.Resolve<IOrderingRepository<ReportOrderSalesProfitMetricsEntity>>();

        await repository.Add(new ReportOrderSalesProfitMetricsEntity(todayStart, 100m, 60m, 40m, 10m, ReportPeriod.Daily), CancellationToken.None);
        await repository.Add(new ReportOrderSalesProfitMetricsEntity(monthStart, 700m, 420m, 280m, 70m, ReportPeriod.Monthly), CancellationToken.None);
        await repository.Add(new ReportOrderSalesProfitMetricsEntity(yearStart, 5000m, 3000m, 2000m, 500m, ReportPeriod.Yearly), CancellationToken.None);
        await repository.Add(new ReportOrderSalesProfitMetricsEntity(DateTimeOffset.MinValue, 9000m, 5400m, 3600m, 900m, ReportPeriod.AllTheTime), CancellationToken.None);

        var result = await Mediator.Send(new GetOrderSalesProfitMetricsQuery());

        result.ShouldBeSuccess();

        result.Value!.ThisDay.TotalRevenue.Should().Be(100m);
        result.Value!.ThisDay.TotalCost.Should().Be(60m);
        result.Value!.ThisDay.TotalProfit.Should().Be(40m);
        result.Value!.ThisDay.TotalReturnAmount.Should().Be(10m);

        result.Value!.ThisMonth.TotalRevenue.Should().Be(700m);
        result.Value!.ThisMonth.TotalCost.Should().Be(420m);

        result.Value!.ThisYear.TotalRevenue.Should().Be(5000m);
        result.Value!.ThisYear.TotalProfit.Should().Be(2000m);

        result.Value!.AllTime.TotalRevenue.Should().Be(9000m);
        result.Value!.AllTime.TotalReturnAmount.Should().Be(900m);
    }
}