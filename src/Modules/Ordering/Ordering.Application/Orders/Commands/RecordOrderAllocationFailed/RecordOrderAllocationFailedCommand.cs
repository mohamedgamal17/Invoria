using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Events;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationFailed;

public sealed class RecordOrderAllocationFailedCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;
    public string OrderNumber { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public List<RecordOrderAllocationFailedLine> ItemErrors { get; init; } = [];

    public static RecordOrderAllocationFailedCommand FromEvent(OrderAllocationFailedIntegrationEvent e) =>
        new()
        {
            OrderId = e.OrderId,
            OrderNumber = e.OrderNumber ?? string.Empty,
            CustomerId = e.CustomerId ?? string.Empty,
            Reason = e.Reason,
            ItemErrors = e.ItemErrors
                .Select(i => new RecordOrderAllocationFailedLine
                {
                    ItemId = i.ProductId,
                    QuantityRequested = i.RequestedQuantity,
                    QuantityAvailable = i.AvailableQuantity,
                    Shortage = i.Shortage
                })
                .ToList()
        };
}

public sealed class RecordOrderAllocationFailedLine
{
    public string ItemId { get; init; } = string.Empty;
    public int QuantityRequested { get; init; }
    public int QuantityAvailable { get; init; }
    public int Shortage { get; init; }
}
