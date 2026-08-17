using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;
using NUnit.Framework;

namespace Invoria.Ordering.Endpoints.Tests.ReportOrderSalesProfitMetrics;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

[TestFixture]
public class ListOrderSalesProfitMetricsEndpointTests : ReportOrderSalesProfitMetricsEndpointTestFixture
{
    [Test]
    public async Task Should_return_paged_metrics_ordered_by_date_descending()
    {
        // Arrange
        var today = DateTimeOffset.Now.Date;
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);

        await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(today, 100m, 60m, 40m, 10m, ReportPeriod.Daily));
        await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(yesterday, 200m, 120m, 80m, 20m, ReportPeriod.Daily));
        await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(twoDaysAgo, 300m, 180m, 120m, 30m, ReportPeriod.Daily));
        await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(today, 900m, 540m, 360m, 90m, ReportPeriod.Monthly));

        // Act
        var response = await Client.GetAsync("/report/orders/sales-profit/metrics?Period=Daily&Skip=0&Length=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ReportOrderSalesProfitMetricsPeriodDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        envelope.Result!.Info.TotalCount.Should().Be(3);
        envelope.Result.Data.Should().HaveCount(3);
        envelope.Result.Data.Should().OnlyContain(x => x.Period == ReportPeriod.Daily);
        envelope.Result.Data.Select(x => x.Date).Should().Equal(today, yesterday, twoDaysAgo);
        envelope.Result.Data.Should().Contain(x => x.TotalProfit == 40m && x.Date == today);
        envelope.Result.Data.Should().Contain(x => x.TotalProfit == 80m && x.Date == yesterday);
        envelope.Result.Data.Should().Contain(x => x.TotalProfit == 120m && x.Date == twoDaysAgo);
    }

    [Test]
    public async Task Should_respect_skip_and_length_for_paging()
    {
        // Arrange
        for (var i = 0; i < 5; i++)
        {
            await ReportRepository.Add(new ReportOrderSalesProfitMetricsEntity(
                DateTimeOffset.Now.Date.AddDays(-i), 100m, 60m, 40m, 10m, ReportPeriod.Daily));
        }

        // Act
        var response = await Client.GetAsync("/report/orders/sales-profit/metrics?Period=Daily&Skip=1&Length=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ReportOrderSalesProfitMetricsPeriodDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();

        envelope.Result!.Info.TotalCount.Should().Be(5);
        envelope.Result.Data.Should().HaveCount(2);
    }
}