using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;

namespace Invoria.Inventory.Application.Allocations.Commands.CreateAllocate;

public sealed class CreateAllocateCommand : ICommand<Empty>
{
    public string Id { get; init; } = string.Empty;

    public string OrderNumber { get; init; } = string.Empty;

    public string CustomerId { get; init; } = string.Empty;

    public List<AllocateOrderLineModel> Items { get; init; } = [];

    public AllocateOrderIntegrationEvent ToEvent() =>
        new()
        {
            Id = Id,
            OrderNumber = OrderNumber,
            CustomerId = CustomerId,
            Items = Items
        };

    public static CreateAllocateCommand FromEvent(AllocateOrderIntegrationEvent message) =>
        new()
        {
            Id = message.Id,
            OrderNumber = message.OrderNumber,
            CustomerId = message.CustomerId,
            Items = message.Items ?? []
        };
}
