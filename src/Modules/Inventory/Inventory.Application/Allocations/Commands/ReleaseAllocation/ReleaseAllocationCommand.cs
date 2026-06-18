using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Allocations.Events;

namespace Invoria.Inventory.Application.Allocations.Commands.ReleaseAllocation;

public sealed class ReleaseAllocationCommand : ICommand<Empty>
{
    public string AllocationId { get; init; } = string.Empty;

    public static ReleaseAllocationCommand FromEvent(ReleaseAllocationIntegrationEvent message) =>
        new() { AllocationId = message.AllocationId };
}
