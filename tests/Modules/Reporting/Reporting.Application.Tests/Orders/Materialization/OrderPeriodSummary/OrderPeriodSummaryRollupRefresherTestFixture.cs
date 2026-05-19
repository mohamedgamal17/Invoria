using Autofac;
using Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Tests.Orders.Materialization.OrderPeriodSummary;

public class OrderPeriodSummaryRollupRefresherTestFixture : ReportingTestFixture
{
    protected ReportingDbContext Db { get; private set; } = null!;

    protected IOrderPeriodSummaryRollupRefresher Refresher { get; private set; } = null!;

    protected override Task BeforeAnyTestRunAsync()
    {
        Db = Scope.Resolve<ReportingDbContext>();
        Refresher = Scope.Resolve<IOrderPeriodSummaryRollupRefresher>();
        return base.BeforeAnyTestRunAsync();
    }

    protected override async Task AfterAnyTestTearDown()
    {
        var summaries = await Db.OrderPeriodSummaries.ToListAsync();
        Db.OrderPeriodSummaries.RemoveRange(summaries);

        var orders = await Db.ReportedOrders.ToListAsync();
        Db.ReportedOrders.RemoveRange(orders);

        await Db.SaveChangesAsync();
        await base.AfterAnyTestTearDown();
    }
}
