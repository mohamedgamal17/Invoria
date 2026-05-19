using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Reporting.Contracts.Dtos;
using Invoria.Reporting.Domain.Orders.StatusSummary;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Orders.Queries.GetOrderStatusSummary;

public sealed class GetOrderStatusSummaryQueryHandler
    : IApplicatonRequestHandler<GetOrderStatusSummaryQuery, IReadOnlyList<OrderStatusSummaryItemDto>>
{
    private readonly IReportingRepository<ReportedOrderStatusByDay> _statusByDayRepository;

    public GetOrderStatusSummaryQueryHandler(
        IReportingRepository<ReportedOrderStatusByDay> statusByDayRepository)
    {
        _statusByDayRepository = statusByDayRepository;
    }

    public async Task<Result<IReadOnlyList<OrderStatusSummaryItemDto>>> Handle(
        GetOrderStatusSummaryQuery request,
        CancellationToken cancellationToken)
    {
        DateOnly? fromDay = request.FromUtc is { } f ? DateOnly.FromDateTime(f.UtcDateTime) : null;
        DateOnly? toDay = request.ToUtc is { } t ? DateOnly.FromDateTime(t.UtcDateTime) : null;

        var query = _statusByDayRepository.AsQuerable().AsNoTracking();

        if (fromDay is not null && toDay is not null)
        {
            query = query.Where(r => r.DayUtc >= fromDay && r.DayUtc <= toDay);
        }

        var rows = await query
            .GroupBy(r => r.OrderStatus)
            .Select(g => new OrderStatusSummaryItemDto
            {
                Status = g.Key,
                Count = g.Sum(x => x.Count)
            })
            .OrderBy(x => x.Status)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<OrderStatusSummaryItemDto>>(rows);
    }
}
