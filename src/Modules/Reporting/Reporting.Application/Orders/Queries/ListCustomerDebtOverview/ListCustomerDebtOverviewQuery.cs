using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Application.Orders.Queries.ListCustomerDebtOverview;

/// <summary>
/// Returns a page of per-customer debt overview rows from materialized <see cref="Domain.Orders.DebtSummary.DebtCustomerSummary"/>.
/// Data may lag live <c>ReportedOrders</c> by up to about five minutes (scheduled refresh interval).
/// </summary>
public sealed class ListCustomerDebtOverviewQuery : PagingParams, IQuery<PagingDto<CustomerDebtOverviewDto>>;
