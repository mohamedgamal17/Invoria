using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Inventory.Application.Batches.Commands.AllocateOrder;

public sealed class AllocateOrderCommand : ICommand<Empty>
{
    public string Id { get; init; } = string.Empty;

    public string OrderNumber { get; init; } = string.Empty;

    public string CustomerId { get; init; } = string.Empty;

    public List<OrderItemModel> Items { get; init; } = [];

    public AllocateOrderIntegrationEvent ToEvent() =>
        new()
        {
            Id = Id,
            OrderNumber = OrderNumber,
            CustomerId = CustomerId,
            Items = Items
        };

    public static AllocateOrderCommand FromEvent(AllocateOrderIntegrationEvent message) =>
        new()
        {
            Id = message.Id,
            OrderNumber = message.OrderNumber,
            CustomerId = message.CustomerId,
            Items = message.Items ?? []
        };
}
