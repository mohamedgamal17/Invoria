using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Repositories;

public sealed class ReportedOrderRepository : ReportingRepository<ReportedOrder>, IReportedOrderRepository
{
    public ReportedOrderRepository(ReportingDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<ReportedOrder?> GetByIdWithGraphAsync(string id, CancellationToken cancellationToken)
    {
        return DbContext.ReportedOrders
            .AsSplitQuery()
            .Include(o => o.Lines)
            .Include(o => o.Payments)
            .Include(o => o.StateTransitions)
            .Include(o => o.FailureDetails)
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task UpsertGraphAsync(ReportedOrder order, CancellationToken cancellationToken)
    {
        var entry = DbContext.Entry(order);
        if (entry.State == EntityState.Detached)
        {
            await DbContext.ReportedOrders.AddAsync(order, cancellationToken);
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
