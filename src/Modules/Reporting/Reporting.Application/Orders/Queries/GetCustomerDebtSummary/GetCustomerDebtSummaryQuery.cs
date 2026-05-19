using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Application.Orders.Queries.GetCustomerDebtSummary;

/// <summary>
/// Returns the materialized per-customer debt overview for <see cref="CustomerId"/>.
/// Data may lag live <c>ReportedOrders</c> by up to about five minutes (scheduled refresh interval).
/// </summary>
public sealed class GetCustomerDebtSummaryQuery : IQuery<CustomerDebtOverviewDto>
{
    public string CustomerId { get; init; } = null!;
}
