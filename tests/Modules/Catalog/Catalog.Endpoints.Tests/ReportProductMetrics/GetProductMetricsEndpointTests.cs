using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;
using NUnit.Framework;

namespace Invoria.Catalog.Endpoints.Tests.ReportProductMetrics
{
    using ReportProductMetricsEntity = Invoria.Catalog.Domain.Products.ReportProductMetrics;

    [TestFixture]
    public class GetProductMetricsEndpointTests : ReportProductMetricsEndpointTestFixture
    {
        [Test]
        public async Task Should_return_current_metrics_for_all_periods()
        {
            // Arrange
            var now = DateTimeOffset.Now;

            var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
            var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
            var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

            await ReportRepository.Add(new ReportProductMetricsEntity(todayStart, 10, ReportPeriod.Daily));
            await ReportRepository.Add(new ReportProductMetricsEntity(monthStart, 25, ReportPeriod.Monthly));
            await ReportRepository.Add(new ReportProductMetricsEntity(yearStart, 40, ReportPeriod.Yearly));
            await ReportRepository.Add(new ReportProductMetricsEntity(DateTimeOffset.MinValue, 100, ReportPeriod.AllTheTime));

            // Act
            var response = await Client.GetAsync("/report/products/creation/overview");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<ReportProductMetricsDto>>();
            envelope.Should().NotBeNull();
            envelope!.IsSuccess.Should().BeTrue();
            envelope.Result.Should().NotBeNull();

            envelope.Result!.ThisDay.Period.Should().Be(ReportPeriod.Daily);
            envelope.Result.ThisDay.TotalCount.Should().Be(10);

            envelope.Result.ThisMonth.Period.Should().Be(ReportPeriod.Monthly);
            envelope.Result.ThisMonth.TotalCount.Should().Be(25);

            envelope.Result.ThisYear.Period.Should().Be(ReportPeriod.Yearly);
            envelope.Result.ThisYear.TotalCount.Should().Be(40);

            envelope.Result.AllTime.Period.Should().Be(ReportPeriod.AllTheTime);
            envelope.Result.AllTime.TotalCount.Should().Be(100);
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
            var response = await Client.GetAsync("/report/products/creation/overview");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<ReportProductMetricsDto>>();
            envelope.Should().NotBeNull();
            envelope!.IsSuccess.Should().BeTrue();
            envelope.Result.Should().NotBeNull();

            envelope.Result!.ThisDay.Period.Should().Be(ReportPeriod.Daily);
            envelope.Result.ThisDay.TotalCount.Should().Be(0);
            envelope.Result.ThisDay.Date.Should().Be(todayStart);

            envelope.Result.ThisMonth.Period.Should().Be(ReportPeriod.Monthly);
            envelope.Result.ThisMonth.TotalCount.Should().Be(0);
            envelope.Result.ThisMonth.Date.Should().Be(monthStart);

            envelope.Result.ThisYear.Period.Should().Be(ReportPeriod.Yearly);
            envelope.Result.ThisYear.TotalCount.Should().Be(0);
            envelope.Result.ThisYear.Date.Should().Be(yearStart);

            envelope.Result.AllTime.Period.Should().Be(ReportPeriod.AllTheTime);
            envelope.Result.AllTime.TotalCount.Should().Be(0);
            envelope.Result.AllTime.Date.Should().Be(DateTimeOffset.MinValue);
        }
    }
}
