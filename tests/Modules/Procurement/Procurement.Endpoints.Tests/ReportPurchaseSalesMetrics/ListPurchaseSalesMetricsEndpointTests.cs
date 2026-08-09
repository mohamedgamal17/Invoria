using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Contracts.Dtos;
using NUnit.Framework;

namespace Invoria.Procurement.Endpoints.Tests.ReportPurchaseSalesMetrics;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

[TestFixture]
public class ListPurchaseSalesMetricsEndpointTests : ReportPurchaseSalesMetricsEndpointTestFixture
{
    [Test]
    public async Task Should_return_paged_metrics_ordered_by_date_descending()
    {
        // Arrange
        var today = DateTimeOffset.Now.Date;
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);

        await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(today, 320m, 300m, 30m, 10m, ReportPeriod.Daily));
        await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(yesterday, 640m, 600m, 60m, 20m, ReportPeriod.Daily));
        await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(twoDaysAgo, 1280m, 1200m, 120m, 40m, ReportPeriod.Daily));
        await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(today, 5000m, 4500m, 500m, 150m, ReportPeriod.Monthly));

        // Act
        var response = await Client.GetAsync("/report/purchase-orders/sales/metrics?Period=Daily&Skip=0&Length=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ReportPurchaseSalesMetricsPeriodDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        envelope.Result!.Info.TotalCount.Should().Be(3);
        envelope.Result.Data.Should().HaveCount(3);
        envelope.Result.Data.Should().OnlyContain(x => x.Period == ReportPeriod.Daily);
        envelope.Result.Data.Select(x => x.Date).Should().Equal(today, yesterday, twoDaysAgo);
        envelope.Result.Data.Should().Contain(x => x.TotalAmount == 320m && x.Date == today);
        envelope.Result.Data.Should().Contain(x => x.TotalAmount == 640m && x.Date == yesterday);
        envelope.Result.Data.Should().Contain(x => x.TotalAmount == 1280m && x.Date == twoDaysAgo);
    }

    [Test]
    public async Task Should_respect_skip_and_length_for_paging()
    {
        // Arrange
        for (var i = 0; i < 5; i++)
        {
            await ReportRepository.Add(new ReportPurchaseSalesMetricsEntity(
                DateTimeOffset.Now.Date.AddDays(-i), 100m, 90m, 10m, 5m, ReportPeriod.Daily));
        }

        // Act
        var response = await Client.GetAsync("/report/purchase-orders/sales/metrics?Period=Daily&Skip=1&Length=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ReportPurchaseSalesMetricsPeriodDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();

        envelope.Result!.Info.TotalCount.Should().Be(5);
        envelope.Result.Data.Should().HaveCount(2);
    }
}