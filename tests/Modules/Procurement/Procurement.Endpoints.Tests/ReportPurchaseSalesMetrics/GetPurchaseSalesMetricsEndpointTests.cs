using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Contracts.Dtos;
using NUnit.Framework;

namespace Invoria.Procurement.Endpoints.Tests.ReportPurchaseSalesMetrics;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

[TestFixture]
public class GetPurchaseSalesMetricsEndpointTests : ReportPurchaseSalesMetricsEndpointTestFixture
{
    [Test]
    public async Task Should_return_current_metrics_for_all_periods()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(todayStart, 320m, 300m, 30m, 10m, ReportPeriod.Daily));
        await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(monthStart, 640m, 600m, 60m, 20m, ReportPeriod.Monthly));
        await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(yearStart, 1280m, 1200m, 120m, 40m, ReportPeriod.Yearly));
        await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(DateTimeOffset.MinValue, 5000m, 4500m, 500m, 150m, ReportPeriod.AllTheTime));

        // Act
        var response = await Client.GetAsync("/report/purchase-orders/sales/overview");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<ReportPurchaseSalesMetricsDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        envelope.Result!.ThisDay.Period.Should().Be(ReportPeriod.Daily);
        envelope.Result.ThisDay.TotalAmount.Should().Be(320m);
        envelope.Result.ThisDay.SubTotal.Should().Be(300m);
        envelope.Result.ThisDay.TaxAmount.Should().Be(30m);
        envelope.Result.ThisDay.DiscountAmount.Should().Be(10m);

        envelope.Result.ThisMonth.Period.Should().Be(ReportPeriod.Monthly);
        envelope.Result.ThisMonth.TotalAmount.Should().Be(640m);
        envelope.Result.ThisMonth.SubTotal.Should().Be(600m);

        envelope.Result.ThisYear.Period.Should().Be(ReportPeriod.Yearly);
        envelope.Result.ThisYear.TotalAmount.Should().Be(1280m);
        envelope.Result.ThisYear.SubTotal.Should().Be(1200m);

        envelope.Result.AllTime.Period.Should().Be(ReportPeriod.AllTheTime);
        envelope.Result.AllTime.TotalAmount.Should().Be(5000m);
        envelope.Result.AllTime.SubTotal.Should().Be(4500m);
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
        var response = await Client.GetAsync("/report/purchase-orders/sales/overview");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<ReportPurchaseSalesMetricsDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        envelope.Result!.ThisDay.Period.Should().Be(ReportPeriod.Daily);
        envelope.Result.ThisDay.TotalAmount.Should().Be(0m);
        envelope.Result.ThisDay.Date.Should().Be(todayStart);

        envelope.Result.ThisMonth.Period.Should().Be(ReportPeriod.Monthly);
        envelope.Result.ThisMonth.TotalAmount.Should().Be(0m);
        envelope.Result.ThisMonth.Date.Should().Be(monthStart);

        envelope.Result.ThisYear.Period.Should().Be(ReportPeriod.Yearly);
        envelope.Result.ThisYear.TotalAmount.Should().Be(0m);
        envelope.Result.ThisYear.Date.Should().Be(yearStart);

        envelope.Result.AllTime.Period.Should().Be(ReportPeriod.AllTheTime);
        envelope.Result.AllTime.TotalAmount.Should().Be(0m);
        envelope.Result.AllTime.Date.Should().Be(DateTimeOffset.MinValue);
    }
}