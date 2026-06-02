using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Endpoints.Tests.Utilities;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Endpoints.Tests.Orders;

[TestFixture]
public sealed class ListOrderPeriodSummaryEndpointTests : ReportingOrdersEndpointTestFixture
{
    [Test]
    public async Task Should_return_paged_day_bucket_for_placed_date_range()
    {
        var may1 = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var may3 = DateTimeOffset.Parse("2026-05-03T10:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateReportedOrder("o1", "c1", OrderStatus.Pending, OrderPaymentStatus.Unpaid, 10m, 0m, may1),
            CreateReportedOrder("o2", "c1", OrderStatus.Pending, OrderPaymentStatus.Unpaid, 20m, 0m, may3));
        await Db.SaveChangesAsync();
        await RefreshReportingRollupsAsync();

        var query = new
        {
            Skip = 0,
            Length = 50,
            From = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc),
            GroupedBy = OrderPeriodSummaryGranularity.Day
        };
        var uri = "/reporting/orders/period-summaries?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderPeriodSummaryDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Info.TotalCount.Should().Be(1);

        var row = envelope.Result.Data!.Single();
        row.PeriodKey.Should().Be("2026-05-01");
        row.OrderCount.Should().Be(1);
        row.GrossRevenue.Should().Be(10m);
    }

    [Test]
    public async Task Should_return_validation_errors_when_length_is_invalid()
    {
        var request = new PagingParams { Skip = 0, Length = 0 };
        var uri = "/reporting/orders/period-summaries?" + QueryStringHelper.ToQueryString(request);

        var response = await Client.GetAsync(uri);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        envelope.Error.Errors.Should().NotBeEmpty();
    }
}
