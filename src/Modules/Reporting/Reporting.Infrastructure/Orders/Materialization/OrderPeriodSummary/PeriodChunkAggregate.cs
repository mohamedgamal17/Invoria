namespace Invoria.Reporting.Infrastructure.Orders.Materialization.OrderPeriodSummary;

public sealed record PeriodChunkAggregate(
    string PeriodKey,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    int OrderCount,
    decimal GrossRevenue,
    decimal NetRevenue,
    int CancelledCount,
    int DeliveredCount);
