using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationFailed;

public sealed class RecordOrderAllocationFailedCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;
    public string OrderNumber { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public List<RecordOrderAllocationFailedLine> ItemErrors { get; init; } = [];
}

public sealed class RecordOrderAllocationFailedLine
{
    public string ItemId { get; init; } = string.Empty;
    public int QuantityRequested { get; init; }
    public int QuantityAvailable { get; init; }
    public int Shortage { get; init; }
}
