using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Events;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;

public sealed class RecordOrderAllocationSucceededCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public string CustomerId { get; init; } = string.Empty;

    public static RecordOrderAllocationSucceededCommand FromEvent(OrderAllocationSucceededIntegrationEvent e) =>
        new()
        {
            OrderId = e.OrderId,
            CustomerId = e.CustomerId
        };
}
