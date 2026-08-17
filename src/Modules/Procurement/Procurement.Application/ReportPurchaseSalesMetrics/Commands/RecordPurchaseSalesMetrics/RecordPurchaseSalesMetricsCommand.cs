using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Commands.RecordPurchaseSalesMetrics;

public sealed class RecordPurchaseSalesMetricsCommand : ICommand<Empty>
{
    public string PurchaseOrderId { get; init; } = string.Empty;

    public DateTimeOffset OccurredOn { get; init; }
}