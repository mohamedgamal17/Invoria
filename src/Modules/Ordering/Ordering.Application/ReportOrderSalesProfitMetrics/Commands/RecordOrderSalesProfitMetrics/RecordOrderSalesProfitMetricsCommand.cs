using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Commands.RecordOrderSalesProfitMetrics;

public sealed class RecordOrderSalesProfitMetricsCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public string AllocationId { get; init; } = string.Empty;

    public DateTimeOffset OccurredOn { get; init; }
}
