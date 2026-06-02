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
public sealed class ListCustomerDebtOverviewEndpointTests : ReportingOrdersEndpointTestFixture
{
    [Test]
    public async Task Should_return_customers_ordered_by_total_outstanding_desc()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateReportedOrder("o1", "c-low", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 10m, 0m, t),
            CreateReportedOrder("o2", "c-high", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 200m, 0m, t),
            CreateReportedOrder("o3", "c-mid", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 50m, 0m, t));
        await Db.SaveChangesAsync();
        await RefreshReportingRollupsAsync();

        var query = new { Skip = 0, Length = 10 };
        var uri = "/reporting/orders/customer-debt-overview?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<CustomerDebtOverviewDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Info.TotalCount.Should().Be(3);

        var rows = envelope.Result.Data!.ToList();
        rows.Should().HaveCount(3);
        rows[0].CustomerId.Should().Be("c-high");
        rows[1].CustomerId.Should().Be("c-mid");
        rows[2].CustomerId.Should().Be("c-low");
    }

    [Test]
    public async Task Should_apply_skip_and_length()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateReportedOrder("o1", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 100m, 0m, t),
            CreateReportedOrder("o2", "c2", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 90m, 0m, t),
            CreateReportedOrder("o3", "c3", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 80m, 0m, t),
            CreateReportedOrder("o4", "c4", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 70m, 0m, t));
        await Db.SaveChangesAsync();
        await RefreshReportingRollupsAsync();

        var query = new { Skip = 1, Length = 2 };
        var uri = "/reporting/orders/customer-debt-overview?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<CustomerDebtOverviewDto>>>();
        envelope!.Result!.Info.TotalCount.Should().Be(4);

        var rows = envelope.Result.Data!.ToList();
        rows.Should().HaveCount(2);
        rows[0].CustomerId.Should().Be("c2");
        rows[1].CustomerId.Should().Be("c3");
    }

    [Test]
    public async Task Should_return_validation_errors_when_length_is_invalid()
    {
        var request = new PagingParams { Skip = 0, Length = 0 };
        var uri = "/reporting/orders/customer-debt-overview?" + QueryStringHelper.ToQueryString(request);

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
