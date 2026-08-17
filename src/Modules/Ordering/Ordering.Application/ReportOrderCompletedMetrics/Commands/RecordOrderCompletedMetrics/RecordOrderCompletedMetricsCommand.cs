using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.ReportOrderCompletedMetrics.Commands.RecordOrderCompletedMetrics;

public sealed class RecordOrderCompletedMetricsCommand : ICommand<Empty>
{
    public DateTimeOffset OccurredOn { get; init; }
}