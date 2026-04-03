using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;

namespace Invoria.Inventory.Application.Batches.Commands.DispatchOrder;

public sealed class DispatchOrderCommand : ICommand<Empty>
{
    public string Id { get; init; } = string.Empty;

    public string OrderNumber { get; init; } = string.Empty;

    public string CustomerId { get; init; } = string.Empty;

    public DateTimeOffset DispatchedAt { get; init; }

    public List<OrderItemModel> Items { get; init; } = [];

    public static DispatchOrderCommand FromEvent(OrderDispatchedIntegrationEvent message) =>
        new()
        {
            Id = message.Id,
            OrderNumber = message.OrderNumber,
            CustomerId = message.CustomerId,
            DispatchedAt = message.DispatchedAt,
            Items = message.Items ?? []
        };
}
