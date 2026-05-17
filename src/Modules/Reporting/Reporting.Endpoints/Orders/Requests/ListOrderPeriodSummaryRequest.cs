using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Endpoints.Orders.Requests;

public sealed class ListOrderPeriodSummaryRequest : PagingParams
{
    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public OrderPeriodSummaryGranularity GroupedBy { get; set; } = OrderPeriodSummaryGranularity.Day;
}
