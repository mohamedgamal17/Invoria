using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;

public class CreateImmediateReturnCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public string AllocationId { get; init; } = string.Empty;

    public List<CreateImmediateReturnLineItem> Lines { get; init; } = [];
}

public class CreateImmediateReturnLineItem
{
    public string OrderItemId { get; init; } = string.Empty;

    public string ProductId { get; init; } = string.Empty;

    public int Quantity { get; init; }
}
