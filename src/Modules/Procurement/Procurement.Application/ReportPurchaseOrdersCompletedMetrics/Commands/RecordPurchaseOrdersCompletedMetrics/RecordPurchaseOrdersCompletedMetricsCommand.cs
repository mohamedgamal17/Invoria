using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Commands.RecordPurchaseOrdersCompletedMetrics;

public sealed class RecordPurchaseOrdersCompletedMetricsCommand : ICommand<Empty>
{
    public DateTimeOffset OccurredOn { get; init; }
}