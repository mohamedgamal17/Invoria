using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;

public sealed class RequestAllocationCommand : ICommand<Empty>
{
    public string AllocationId { get; init; } = string.Empty;
}
