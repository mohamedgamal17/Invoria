using Invoria.Reporting.Domain.Orders;

namespace Invoria.Reporting.Domain.Repositories;

/// <summary>
/// Read/write access to the denormalized order projection graph.
/// Infrastructure implements idempotent upsert semantics (replace graph by order id).
/// </summary>
public interface IReportedOrderRepository : IReportingRepository<ReportedOrder>
{
    Task<ReportedOrder?> GetByIdWithGraphAsync(string id, CancellationToken cancellationToken);

    Task UpsertGraphAsync(ReportedOrder order, CancellationToken cancellationToken);
}
