using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Reporting.Contracts.Dtos;

namespace Invoria.Reporting.Application.Orders.Queries.GetOrderStatusSummary;

/// <summary>
/// Returns counts of reported orders grouped by current order status from the materialized order-status-by-day summary.
/// When <see cref="FromUtc"/> and <see cref="ToUtc"/> are set, only orders whose <c>CreatedAt</c> (UTC calendar day)
/// falls in that inclusive range are included; when both are omitted, all materialized rows are aggregated (all time).
/// Data may lag live <c>ReportedOrders</c> by up to about five minutes (scheduled refresh interval).
/// </summary>
public sealed class GetOrderStatusSummaryQuery : IQuery<IReadOnlyList<OrderStatusSummaryItemDto>>
{
    public DateTimeOffset? FromUtc { get; init; }

    public DateTimeOffset? ToUtc { get; init; }
}
