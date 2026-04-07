using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Events;

namespace Invoria.Ordering.Application.Orders.Commands.CompleteRefusalAfterInventoryReleased;

public sealed class CompleteRefusalAfterInventoryReleasedCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public string OrderNumber { get; init; } = string.Empty;

    public string CustomerId { get; init; } = string.Empty;

    public static CompleteRefusalAfterInventoryReleasedCommand FromEvent(OrderRefusalInventoryReleasedIntegrationEvent e) =>
        new()
        {
            OrderId = e.OrderId,
            OrderNumber = e.OrderNumber,
            CustomerId = e.CustomerId
        };
}
