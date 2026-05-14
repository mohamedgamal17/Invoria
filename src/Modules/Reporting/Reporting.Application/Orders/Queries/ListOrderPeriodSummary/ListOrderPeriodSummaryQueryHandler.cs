using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Application.Orders.Queries.ListOrderPeriodSummary;

/// <summary>
/// Returns a page of materialized <see cref="OrderPeriodSummaryDto"/> rows (UTC placed date / CreatedAt rollup only;
/// global rollup; not filtered by customer or status).
/// </summary>
public sealed class ListOrderPeriodSummaryQueryHandler
    : IApplicatonRequestHandler<ListOrderPeriodSummaryQuery, PagingDto<OrderPeriodSummaryDto>>
{
    private readonly IOrderPeriodSummaryRollupReader _periodSummaryReader;

    public ListOrderPeriodSummaryQueryHandler(IOrderPeriodSummaryRollupReader periodSummaryReader)
    {
        _periodSummaryReader = periodSummaryReader;
    }

    public async Task<Result<PagingDto<OrderPeriodSummaryDto>>> Handle(
        ListOrderPeriodSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var (fromDay, toDay) = ListOrderPeriodSummaryDateRange.Resolve(request.From, request.To, utcNow);

        var (items, totalCount) = await _periodSummaryReader.GetPeriodSummariesPageAsync(
            request.GroupedBy,
            fromDay,
            toDay,
            request.Skip,
            request.Length,
            cancellationToken);

        return Result.Success(new PagingDto<OrderPeriodSummaryDto>
        {
            Data = items,
            Info = new PagingInfoDto
            {
                Skip = request.Skip,
                Length = request.Length,
                TotalCount = totalCount
            }
        });
    }
}
