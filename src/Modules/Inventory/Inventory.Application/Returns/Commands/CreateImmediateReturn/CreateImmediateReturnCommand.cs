using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Returns.Events;

namespace Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;

public class CreateImmediateReturnCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public string AllocationId { get; init; } = string.Empty;

    public List<CreateImmediateReturnLineItem> Lines { get; init; } = [];

    public static CreateImmediateReturnCommand FromEvent(CreateImmediateReturnIntegrationEvent message) =>
        new()
        {
            OrderId = message.OrderId,
            AllocationId = message.AllocationId,
            Lines = (message.Lines ?? []).Select(l => new CreateImmediateReturnLineItem
            {
                OrderItemId = l.OrderItemId,
                ProductId = l.ProductId,
                Quantity = l.Quantity
            }).ToList()
        };
}

public class CreateImmediateReturnLineItem
{
    public string OrderItemId { get; init; } = string.Empty;

    public string ProductId { get; init; } = string.Empty;

    public int Quantity { get; init; }
}
