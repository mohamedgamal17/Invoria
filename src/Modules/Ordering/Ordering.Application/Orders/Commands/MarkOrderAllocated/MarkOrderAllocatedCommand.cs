using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.Orders.Commands.MarkOrderAllocated;

public sealed class MarkOrderAllocatedCommand : ICommand<Empty>
{
    public MarkOrderAllocatedCommand(string orderId)
    {
        OrderId = orderId;
    }

    public string OrderId { get; }
}
