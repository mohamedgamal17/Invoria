using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;
using NUnit.Framework;

namespace Invoria.Ordering.Endpoints.Tests.ReportOrderSalesProfitMetrics;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

[TestFixture]
public class GetOrderSalesProfitMetricsEndpointTests : ReportOrderSalesProfitMetricsEndpointTestFixture
{
    [Test]
    public async Task Should_return_current_metrics_for_all_periods()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(todayStart, 100m, 60m, 40m, 10m, ReportPeriod.Daily));
        await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(monthStart, 700m, 420m, 280m, 70m, ReportPeriod.Monthly));
        await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(yearStart, 5000m, 3000m, 2000m, 500m, ReportPeriod.Yearly));
        await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(DateTimeOffset.MinValue, 9000m, 5400m, 3600m, 900m, ReportPeriod.AllTheTime));

        // Act
        var response = await Client.GetAsync("/report/orders/sales-profit/overview");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<ReportOrderSalesProfitMetricsDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        envelope.Result!.ThisDay.Period.Should().Be(ReportPeriod.Daily);
        envelope.Result.ThisDay.TotalRevenue.Should().Be(100m);
        envelope.Result.ThisDay.TotalCost.Should().Be(60m);
        envelope.Result.ThisDay.TotalProfit.Should().Be(40m);
        envelope.Result.ThisDay.TotalReturnAmount.Should().Be(10m);

        envelope.Result.ThisMonth.Period.Should().Be(ReportPeriod.Monthly);
        envelope.Result.ThisMonth.TotalRevenue.Should().Be(700m);
        envelope.Result.ThisMonth.TotalCost.Should().Be(420m);
        envelope.Result.ThisMonth.TotalProfit.Should().Be(280m);
        envelope.Result.ThisMonth.TotalReturnAmount.Should().Be(70m);

        envelope.Result.ThisYear.Period.Should().Be(ReportPeriod.Yearly);
        envelope.Result.ThisYear.TotalRevenue.Should().Be(5000m);
        envelope.Result.ThisYear.TotalProfit.Should().Be(2000m);

        envelope.Result.AllTime.Period.Should().Be(ReportPeriod.AllTheTime);
        envelope.Result.AllTime.TotalRevenue.Should().Be(9000m);
        envelope.Result.AllTime.TotalReturnAmount.Should().Be(900m);
    }

    [Test]
    public async Task Should_return_zeros_when_no_metrics_exist()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        // Act
        var response = await Client.GetAsync("/report/orders/sales-profit/overview");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<ReportOrderSalesProfitMetricsDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        envelope.Result!.ThisDay.Period.Should().Be(ReportPeriod.Daily);
        envelope.Result.ThisDay.TotalRevenue.Should().Be(0m);
        envelope.Result.ThisDay.TotalCost.Should().Be(0m);
        envelope.Result.ThisDay.TotalProfit.Should().Be(0m);
        envelope.Result.ThisDay.TotalReturnAmount.Should().Be(0m);
        envelope.Result.ThisDay.Date.Should().Be(todayStart);

        envelope.Result.ThisMonth.Period.Should().Be(ReportPeriod.Monthly);
        envelope.Result.ThisMonth.TotalRevenue.Should().Be(0m);
        envelope.Result.ThisMonth.Date.Should().Be(monthStart);

        envelope.Result.ThisYear.Period.Should().Be(ReportPeriod.Yearly);
        envelope.Result.ThisYear.TotalRevenue.Should().Be(0m);
        envelope.Result.ThisYear.Date.Should().Be(yearStart);

        envelope.Result.AllTime.Period.Should().Be(ReportPeriod.AllTheTime);
        envelope.Result.AllTime.TotalRevenue.Should().Be(0m);
        envelope.Result.AllTime.Date.Should().Be(DateTimeOffset.MinValue);
    }
}