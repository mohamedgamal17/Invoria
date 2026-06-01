using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.Orders.Commands.CompleteReopenAfterInventoryReleased;

public sealed class CompleteReopenAfterInventoryReleasedCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public string OrderNumber { get; init; } = string.Empty;

    public string CustomerId { get; init; } = string.Empty;
}
