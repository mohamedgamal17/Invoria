using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Queries.ListOrderSalesProfitMetrics;
using Invoria.Ordering.Application.Tests.Orders;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Tests.ReportOrderSalesProfitMetrics.Queries.ListOrderSalesProfitMetrics;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

[TestFixture]
public class ListOrderSalesProfitMetricsQueryHandlerTests : OrderTestFixture
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
    public async Task Should_filter_by_period_and_apply_paging_ordered_by_date_desc()
    {
        var repository = Scope.Resolve<IOrderingRepository<ReportOrderSalesProfitMetricsEntity>>();

        await repository.Add(new ReportOrderSalesProfitMetricsEntity(
            new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
            100m, 60m, 40m, 10m, ReportPeriod.Daily), CancellationToken.None);
        await repository.Add(new ReportOrderSalesProfitMetricsEntity(
            new DateTimeOffset(2024, 5, 2, 0, 0, 0, TimeSpan.Zero),
            200m, 120m, 80m, 20m, ReportPeriod.Daily), CancellationToken.None);
        await repository.Add(new ReportOrderSalesProfitMetricsEntity(
            new DateTimeOffset(2024, 5, 3, 0, 0, 0, TimeSpan.Zero),
            300m, 180m, 120m, 30m, ReportPeriod.Daily), CancellationToken.None);
        await repository.Add(new ReportOrderSalesProfitMetricsEntity(
            new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
            50m, 30m, 20m, 5m, ReportPeriod.Monthly), CancellationToken.None);

        var result = await Mediator.Send(new ListOrderSalesProfitMetricsQuery
        {
            Period = ReportPeriod.Daily,
            Skip = 1,
            Length = 2
        });

        result.ShouldBeSuccess();

        result.Value!.Info.TotalCount.Should().Be(3);
        result.Value!.Info.Skip.Should().Be(1);
        result.Value!.Info.Length.Should().Be(2);

        var data = result.Value!.Data.ToList();
        data.Should().HaveCount(2);

        data[0].Date.Should().Be(new DateTimeOffset(2024, 5, 2, 0, 0, 0, TimeSpan.Zero));
        data[0].TotalRevenue.Should().Be(200m);
        data[0].TotalCost.Should().Be(120m);
        data[0].TotalProfit.Should().Be(80m);
        data[0].TotalReturnAmount.Should().Be(20m);
        data[0].Period.Should().Be(ReportPeriod.Daily);

        data[1].Date.Should().Be(new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero));
        data[1].TotalRevenue.Should().Be(100m);
        data[1].TotalCost.Should().Be(60m);
    }

    [Test]
    public async Task Should_return_empty_page_when_no_rows_match_period()
    {
        var result = await Mediator.Send(new ListOrderSalesProfitMetricsQuery
        {
            Period = ReportPeriod.Yearly
        });

        result.ShouldBeSuccess();

        result.Value!.Data.Should().BeEmpty();
        result.Value!.Info.TotalCount.Should().Be(0);
    }
}