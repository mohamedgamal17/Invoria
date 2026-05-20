using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Domain.Orders.OrderPeriodSummary;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Orders.Queries.ListOrderPeriodSummary;

/// <summary>
/// Returns a page of materialized <see cref="OrderPeriodSummaryDto"/> rows (UTC placed date / CreatedAt rollup only;
/// global rollup; not filtered by customer or status).
/// </summary>
public sealed class ListOrderPeriodSummaryQueryHandler
    : IApplicatonRequestHandler<ListOrderPeriodSummaryQuery, PagingDto<OrderPeriodSummaryDto>>
{
    /// <summary>Stored clock dimension; placed-only rollups always use <c>0</c>.</summary>
    private const int PlacedDateField = 0;

    private readonly IReportingRepository<OrderPeriodSummary> _orderPeriodSummaryRepository;

    public ListOrderPeriodSummaryQueryHandler(
        IReportingRepository<OrderPeriodSummary> orderPeriodSummaryRepository)
    {
        _orderPeriodSummaryRepository = orderPeriodSummaryRepository;
    }

    public async Task<Result<PagingDto<OrderPeriodSummaryDto>>> Handle(
        ListOrderPeriodSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var (fromDay, toDay) = ListOrderPeriodSummaryDateRange.Resolve(request.From, request.To, utcNow);
        var gran = request.GroupedBy.ToString();

        var query = _orderPeriodSummaryRepository
            .AsQuerable()
            .AsNoTracking()
            .Where(x => x.Granularity == gran && x.DateField == PlacedDateField)
            .Where(x => x.PeriodStart <= toDay && x.PeriodEnd >= fromDay)
            .OrderByDescending(x => x.PeriodStart)
            .ThenByDescending(x => x.PeriodKey)
            .Select(x => new OrderPeriodSummaryDto
            {
                Granularity = x.Granularity,
                PeriodKey = x.PeriodKey,
                PeriodStart = x.PeriodStart,
                PeriodEnd = x.PeriodEnd,
                OrderCount = x.OrderCount,
                GrossRevenue = x.GrossRevenue,
                NetRevenue = x.NetRevenue,
                DiscountAmount = x.DiscountAmount,
                CancelledCount = x.CancelledCount,
                DeliveredCount = x.DeliveredCount
            });

        var paging = await query.ToPaged(request.Skip, request.Length);

        return Result.Success(paging);
    }
}
