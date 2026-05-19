using Autofac;
using Invoria.Reporting.Application.Orders.Materialization.DebtSummary;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Tests.Orders.Materialization.DebtSummary;

public class DebtSummaryRollupRefresherTestFixture : ReportingTestFixture
{
    protected ReportingDbContext Db { get; private set; } = null!;

    protected IDebtSummaryRollupRefresher Refresher { get; private set; } = null!;

    protected override Task BeforeAnyTestRunAsync()
    {
        Db = Scope.Resolve<ReportingDbContext>();
        Refresher = Scope.Resolve<IDebtSummaryRollupRefresher>();
        return base.BeforeAnyTestRunAsync();
    }

    protected override async Task AfterAnyTestTearDown()
    {
        var summaries = await Db.DebtSummaries.ToListAsync();
        Db.DebtSummaries.RemoveRange(summaries);

        var orders = await Db.ReportedOrders.ToListAsync();
        Db.ReportedOrders.RemoveRange(orders);

        await Db.SaveChangesAsync();
        await base.AfterAnyTestTearDown();
    }
}
