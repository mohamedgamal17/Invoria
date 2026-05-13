using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Infrastructure.EntityFramework.Repositories;

public sealed class ReportedOrderRepository : IReportedOrderRepository
{
    private readonly ReportingDbContext _dbContext;

    public ReportedOrderRepository(ReportingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ReportedOrder?> GetByIdWithGraphAsync(string id, CancellationToken cancellationToken)
    {
        return _dbContext.ReportedOrders
            .AsSplitQuery()
            .Include(o => o.Lines)
            .Include(o => o.Payments)
            .Include(o => o.StateTransitions)
            .Include(o => o.FailureDetails)
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task UpsertGraphAsync(ReportedOrder order, CancellationToken cancellationToken)
    {
        var entry = _dbContext.Entry(order);
        if (entry.State == EntityState.Detached)
        {
            await _dbContext.ReportedOrders.AddAsync(order, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
