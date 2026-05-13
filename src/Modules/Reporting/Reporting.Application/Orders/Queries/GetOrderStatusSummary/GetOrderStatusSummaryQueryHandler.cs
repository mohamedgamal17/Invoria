using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Invoria.Reporting.Contracts.Dtos;

namespace Invoria.Reporting.Application.Orders.Queries.GetOrderStatusSummary;

public sealed class GetOrderStatusSummaryQueryHandler
    : IApplicatonRequestHandler<GetOrderStatusSummaryQuery, IReadOnlyList<OrderStatusSummaryItemDto>>
{
    private readonly IReportedOrderStatusSummaryRollupReader _rollupReader;

    public GetOrderStatusSummaryQueryHandler(IReportedOrderStatusSummaryRollupReader rollupReader)
    {
        _rollupReader = rollupReader;
    }

    public async Task<Result<IReadOnlyList<OrderStatusSummaryItemDto>>> Handle(
        GetOrderStatusSummaryQuery request,
        CancellationToken cancellationToken)
    {
        DateOnly? fromDay = request.FromUtc is { } f ? DateOnly.FromDateTime(f.UtcDateTime) : null;
        DateOnly? toDay = request.ToUtc is { } t ? DateOnly.FromDateTime(t.UtcDateTime) : null;

        var rows = await _rollupReader.GetAggregatedByStatusAsync(fromDay, toDay, cancellationToken);

        return Result.Success<IReadOnlyList<OrderStatusSummaryItemDto>>(rows);
    }
}
