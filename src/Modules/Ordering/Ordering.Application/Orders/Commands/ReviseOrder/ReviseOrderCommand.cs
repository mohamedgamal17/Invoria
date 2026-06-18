using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.Orders.Commands.ReviseOrder;

public sealed class ReviseOrderCommand : ICommand<Empty>
{
    public ReviseOrderCommand(string orderId)
    {
        OrderId = orderId;
    }

    public string OrderId { get; }
}
