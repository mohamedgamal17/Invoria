using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Application.Orders.Queries.GetDebtOverview;

/// <summary>
/// Returns the materialized global debt overview snapshot from <see cref="Domain.Orders.DebtSummary.DebtGlobalSummary"/>.
/// Data may lag live <c>ReportedOrders</c> by up to about five minutes (scheduled refresh interval).
/// </summary>
public sealed class GetDebtOverviewQuery : IQuery<DebtOverviewDto>;
