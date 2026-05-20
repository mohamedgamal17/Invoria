using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Endpoints.Tests.Utilities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Contracts.Dtos;

namespace Invoria.Reporting.Endpoints.Tests.Orders;

[TestFixture]
public sealed class GetOrderStatusSummaryEndpointTests : ReportingOrdersEndpointTestFixture
{
    [Test]
    public async Task Should_return_status_counts_for_date_range()
    {
        var may1 = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var may2 = DateTimeOffset.Parse("2026-05-02T08:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateReportedOrder("o1", "c1", OrderStatus.Pending, OrderPaymentStatus.Unpaid, 1m, 0m, may1),
            CreateReportedOrder("o2", "c1", OrderStatus.Pending, OrderPaymentStatus.Unpaid, 1m, 0m, may1),
            CreateReportedOrder("o3", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 1m, 0m, may2));
        await Db.SaveChangesAsync();
        await RefreshReportingRollupsAsync();

        var query = new
        {
            FromUtc = DateTimeOffset.Parse("2026-05-01T00:00:00Z"),
            ToUtc = DateTimeOffset.Parse("2026-05-02T23:59:59Z")
        };
        var uri = "/reporting/orders/status-summary?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<IReadOnlyList<OrderStatusSummaryItemDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        var rows = envelope.Result!.ToList();
        rows.Should().HaveCount(2);
        rows.Should().Contain(x => x.Status == OrderStatus.Pending && x.Count == 2);
        rows.Should().Contain(x => x.Status == OrderStatus.Completed && x.Count == 1);
    }

    [Test]
    public async Task Should_return_all_status_counts_when_date_range_omitted()
    {
        var may1 = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var may2 = DateTimeOffset.Parse("2026-05-02T08:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateReportedOrder("o1", "c1", OrderStatus.Pending, OrderPaymentStatus.Unpaid, 1m, 0m, may1),
            CreateReportedOrder("o2", "c1", OrderStatus.Pending, OrderPaymentStatus.Unpaid, 1m, 0m, may1),
            CreateReportedOrder("o3", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 1m, 0m, may2));
        await Db.SaveChangesAsync();
        await RefreshReportingRollupsAsync();

        var response = await Client.GetAsync("/reporting/orders/status-summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<IReadOnlyList<OrderStatusSummaryItemDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        var rows = envelope.Result!.ToList();
        rows.Should().HaveCount(2);
        rows.Should().Contain(x => x.Status == OrderStatus.Pending && x.Count == 2);
        rows.Should().Contain(x => x.Status == OrderStatus.Completed && x.Count == 1);
    }
}
