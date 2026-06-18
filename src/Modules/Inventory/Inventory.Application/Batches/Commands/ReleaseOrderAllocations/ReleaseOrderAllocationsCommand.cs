using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Inventory.Application.Batches.Commands.ReleaseOrderAllocations;

public sealed class ReleaseOrderAllocationsCommand : ICommand<Empty>
{
    public string Id { get; init; } = string.Empty;

    public string OrderNumber { get; init; } = string.Empty;

    public string CustomerId { get; init; } = string.Empty;

    public List<OrderItemModel> Items { get; init; } = [];

    public static ReleaseOrderAllocationsCommand FromEvent(ReleaseOrderAllocationsIntegrationEvent message) =>
        new()
        {
            Id = message.Id,
            OrderNumber = message.OrderNumber,
            CustomerId = message.CustomerId,
            Items = message.Items ?? []
        };
}
