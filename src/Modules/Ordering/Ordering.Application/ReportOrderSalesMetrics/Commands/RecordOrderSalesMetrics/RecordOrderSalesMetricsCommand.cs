using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.ReportOrderSalesMetrics.Commands.RecordOrderSalesMetrics;

public sealed class RecordOrderSalesMetricsCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public DateTimeOffset OccurredOn { get; init; }
}
