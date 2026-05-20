using Autofac;
using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Tests.Orders.Materialization.StatusSummary;

public class ReportedOrderStatusSummaryRollupRefresherTestFixture : ReportingTestFixture
{
    protected ReportingDbContext Db { get; private set; } = null!;

    protected IReportedOrderStatusSummaryRollupRefresher Refresher { get; private set; } = null!;

    protected override Task BeforeAnyTestRunAsync()
    {
        Db = Scope.Resolve<ReportingDbContext>();
        Refresher = Scope.Resolve<IReportedOrderStatusSummaryRollupRefresher>();
        return base.BeforeAnyTestRunAsync();
    }

    protected override async Task AfterAnyTestTearDown()
    {
        var rollups = await Db.ReportedOrderStatusByDays.ToListAsync();
        Db.ReportedOrderStatusByDays.RemoveRange(rollups);

        var orders = await Db.ReportedOrders.ToListAsync();
        Db.ReportedOrders.RemoveRange(orders);

        await Db.SaveChangesAsync();
        await base.AfterAnyTestTearDown();
    }
}
