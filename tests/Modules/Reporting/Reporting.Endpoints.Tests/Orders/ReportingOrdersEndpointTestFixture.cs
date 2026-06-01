using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Application.Orders.Materialization.DebtSummary;
using Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Reporting.Endpoints.Tests.Orders;

public abstract class ReportingOrdersEndpointTestFixture : ReportingTestFixture
{
    protected ReportingDbContext Db => Scope.ServiceProvider.GetRequiredService<ReportingDbContext>();

    [TearDown]
    public async Task TearDownReportingDataAsync()
    {
        Db.ChangeTracker.Clear();

        await Db.DebtSummaries.ExecuteDeleteAsync();
        await Db.OrderPeriodSummaries.ExecuteDeleteAsync();
        await Db.ReportedOrderStatusByDays.ExecuteDeleteAsync();
        await Db.ReportedOrders.ExecuteDeleteAsync();
    }

    protected static ReportedOrder CreateReportedOrder(
        string id,
        string customerId,
        OrderStatus orderStatus,
        OrderPaymentStatus paymentStatus,
        decimal totalOrderAmount,
        decimal amountPaid,
        DateTimeOffset createdAt,
        OrderPaymentType paymentType = OrderPaymentType.Debt) =>
        new()
        {
            Id = id,
            OrderNumber = id,
            CustomerId = customerId,
            OrderStatus = orderStatus,
            PaymentType = paymentType,
            PaymentStatus = paymentStatus,
            TotalOrderAmount = totalOrderAmount,
            AmountPaid = amountPaid,
            AmountOutstanding = totalOrderAmount - amountPaid,
            ReplicationVersion = 1,
            SourceLastKnownAt = createdAt,
            CreatedAt = createdAt,
            Lines = new List<ReportedOrderLine>()
        };

    protected async Task RefreshReportingRollupsAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = Services.CreateAsyncScope();

        var statusRefresher = scope.ServiceProvider.GetRequiredService<IReportedOrderStatusSummaryRollupRefresher>();
        await statusRefresher.RefreshAsync(cancellationToken);

        var periodRefresher = scope.ServiceProvider.GetRequiredService<IOrderPeriodSummaryRollupRefresher>();
        await periodRefresher.RefreshAsync(cancellationToken);

        var debtRefresher = scope.ServiceProvider.GetRequiredService<IDebtSummaryRollupRefresher>();
        await debtRefresher.RefreshAsync(cancellationToken);
    }
}
