using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Events;

namespace Invoria.Ordering.Application.Orders.Commands.CompleteReopenAfterInventoryReleased;

public sealed class CompleteReopenAfterInventoryReleasedCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public string OrderNumber { get; init; } = string.Empty;

    public string CustomerId { get; init; } = string.Empty;

    public static CompleteReopenAfterInventoryReleasedCommand FromEvent(OrderReopenInventoryReleasedIntegrationEvent e) =>
        new()
        {
            OrderId = e.OrderId,
            OrderNumber = e.OrderNumber,
            CustomerId = e.CustomerId
        };
}
