using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocation;

public sealed class RecordOrderAllocationCommand : ICommand<Empty>
{
    public RecordOrderAllocationCommand(string orderId, string allocationId)
    {
        OrderId = orderId;
        AllocationId = allocationId;
    }

    public string OrderId { get; }

    public string AllocationId { get; }
}
