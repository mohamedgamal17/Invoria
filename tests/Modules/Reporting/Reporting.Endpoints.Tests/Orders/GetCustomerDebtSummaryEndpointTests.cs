using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Endpoints.Tests.Orders;

[TestFixture]
public sealed class GetCustomerDebtSummaryEndpointTests : ReportingOrdersEndpointTestFixture
{
    [Test]
    public async Task Should_return_customer_debt_overview_when_found()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateReportedOrder("o1", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 100m, 0m, t),
            CreateReportedOrder("o2", "c1", OrderStatus.Completed, OrderPaymentStatus.Partial, 50m, 20m, t),
            CreateReportedOrder("o3", "c2", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 40m, 0m, t));
        await Db.SaveChangesAsync();
        await RefreshReportingRollupsAsync();

        var response = await Client.GetAsync("/reporting/orders/customer-debt-overview/c1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<CustomerDebtOverviewDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.CustomerId.Should().Be("c1");
        envelope.Result.TotalOutstanding.Should().Be(130m);
        envelope.Result.DebtOrderCount.Should().Be(2);
    }

    [Test]
    public async Task Should_return_zeroed_defaults_when_customer_has_no_debt_row()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        Db.ReportedOrders.Add(
            CreateReportedOrder("o1", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 40m, 0m, t));
        await Db.SaveChangesAsync();
        await RefreshReportingRollupsAsync();

        var response = await Client.GetAsync("/reporting/orders/customer-debt-overview/unknown");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<CustomerDebtOverviewDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.CustomerId.Should().Be("unknown");
        envelope.Result.TotalOutstanding.Should().Be(0m);
        envelope.Result.DebtOrderCount.Should().Be(0);
    }
}
