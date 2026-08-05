using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.CustomerManagement.Contracts.Dtos;
using NUnit.Framework;

namespace Invoria.CustomerManagement.Endpoints.Tests.ReportCustomerMetrics
{
    using ReportCustomerMetricsEntity = Invoria.CustomerManagement.Domain.Customers.ReportCustomerMetrics;

    [TestFixture]
    public class ListCustomerMetricsEndpointTests : ReportCustomerMetricsEndpointTestFixture
    {
        [Test]
        public async Task Should_return_paged_metrics_ordered_by_date_descending()
        {
            // Arrange
            var today = DateTimeOffset.Now.Date;
            var yesterday = today.AddDays(-1);
            var twoDaysAgo = today.AddDays(-2);

            await ReportRepository.Add(new ReportCustomerMetricsEntity(today, 5, ReportPeriod.Daily));
            await ReportRepository.Add(new ReportCustomerMetricsEntity(yesterday, 7, ReportPeriod.Daily));
            await ReportRepository.Add(new ReportCustomerMetricsEntity(twoDaysAgo, 3, ReportPeriod.Daily));
            await ReportRepository.Add(new ReportCustomerMetricsEntity(today, 9, ReportPeriod.Monthly));

            // Act
            var response = await Client.GetAsync("/report/customers/creation/metrics?Period=Daily&Skip=0&Length=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ReportCustomerMetricsPeriodDto>>>();
            envelope.Should().NotBeNull();
            envelope!.IsSuccess.Should().BeTrue();
            envelope.Result.Should().NotBeNull();

            envelope.Result!.Info.TotalCount.Should().Be(3);
            envelope.Result.Data.Should().HaveCount(3);
            envelope.Result.Data.Should().OnlyContain(x => x.Period == ReportPeriod.Daily);
            envelope.Result.Data.Select(x => x.Date).Should().Equal(today, yesterday, twoDaysAgo);
            envelope.Result.Data.Should().Contain(x => x.TotalCount == 5 && x.Date == today);
            envelope.Result.Data.Should().Contain(x => x.TotalCount == 7 && x.Date == yesterday);
            envelope.Result.Data.Should().Contain(x => x.TotalCount == 3 && x.Date == twoDaysAgo);
        }

        [Test]
        public async Task Should_respect_skip_and_length_for_paging()
        {
            // Arrange
            for (var i = 0; i < 5; i++)
            {
                await ReportRepository.Add(new ReportCustomerMetricsEntity(
                    DateTimeOffset.Now.Date.AddDays(-i), i + 1, ReportPeriod.Daily));
            }

            // Act
            var response = await Client.GetAsync("/report/customers/creation/metrics?Period=Daily&Skip=1&Length=2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ReportCustomerMetricsPeriodDto>>>();
            envelope.Should().NotBeNull();
            envelope!.IsSuccess.Should().BeTrue();

            envelope.Result!.Info.TotalCount.Should().Be(5);
            envelope.Result.Data.Should().HaveCount(2);
        }
    }
}