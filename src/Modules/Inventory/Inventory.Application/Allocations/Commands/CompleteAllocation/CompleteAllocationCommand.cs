using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Orders.Events;

namespace Invoria.Inventory.Application.Allocations.Commands.CompleteAllocation;

public sealed class CompleteAllocationCommand : ICommand<Empty>
{
    public string AllocationId { get; init; } = string.Empty;

    public static CompleteAllocationCommand FromEvent(OrderCompletedIntegrationEvent message) =>
        new() { AllocationId = message.AllocationId! };
}
