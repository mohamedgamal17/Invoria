using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Application.Orders.Queries.ListOrderPeriodSummary;

public sealed class ListOrderPeriodSummaryQuery : PagingParams, IQuery<PagingDto<OrderPeriodSummaryDto>>
{
    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public OrderPeriodSummaryGranularity GroupedBy { get; init; } = OrderPeriodSummaryGranularity.Day;
}
