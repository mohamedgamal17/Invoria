namespace Invoria.Reporting.Infrastructure.Orders.Materialization.DebtSummary;

public sealed record DebtCustomerChunkAggregate(
    string CustomerId,
    decimal TotalOutstanding,
    decimal TotalPaid,
    decimal TotalOrderValue,
    int DebtOrderCount,
    int PartiallyPaidCount,
    int UnpaidCount,
    DateTimeOffset? OldestDebtDate,
    decimal OldestDebtAmount);
