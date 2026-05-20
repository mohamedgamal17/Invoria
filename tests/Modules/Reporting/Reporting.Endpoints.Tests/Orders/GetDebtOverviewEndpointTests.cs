using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Endpoints.Tests.Orders;

[TestFixture]
public sealed class GetDebtOverviewEndpointTests : ReportingOrdersEndpointTestFixture
{
    [Test]
    public async Task Should_return_global_debt_overview_after_refresh()
    {
        var t = DateTimeOffset.Parse("2026-05-01T12:00:00Z");

        Db.ReportedOrders.AddRange(
            CreateReportedOrder("o1", "c1", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 100m, 0m, t),
            CreateReportedOrder("o2", "c1", OrderStatus.Completed, OrderPaymentStatus.Partial, 50m, 20m, t),
            CreateReportedOrder("o3", "c2", OrderStatus.Completed, OrderPaymentStatus.Unpaid, 40m, 0m, t));
        await Db.SaveChangesAsync();
        await RefreshReportingRollupsAsync();

        var response = await Client.GetAsync("/reporting/orders/debt-overview");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<DebtOverviewDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();

        var dto = envelope.Result!;
        dto.DebtOrderCount.Should().Be(3);
        dto.TotalOrderValue.Should().Be(190m);
        dto.TotalPaid.Should().Be(20m);
        dto.TotalOutstanding.Should().Be(170m);
        dto.UnpaidCount.Should().Be(2);
        dto.PartiallyPaidCount.Should().Be(1);
        dto.CollectionRate.Should().Be(10.53m);
    }
}
