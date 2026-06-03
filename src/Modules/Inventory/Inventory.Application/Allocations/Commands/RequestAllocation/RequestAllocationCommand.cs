using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Allocations.Events;

namespace Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;

public sealed class RequestAllocationCommand : ICommand<Empty>
{
    public string AllocationId { get; init; } = string.Empty;

    public static RequestAllocationCommand FromEvent(RequestAllocationIntegrationEvent message) =>
        new() { AllocationId = message.AllocationId };
}
