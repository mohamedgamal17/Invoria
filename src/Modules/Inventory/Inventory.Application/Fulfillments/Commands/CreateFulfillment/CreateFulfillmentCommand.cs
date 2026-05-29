using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Inventory.Application.Fulfillments.Commands.CreateFulfillment;

public sealed class CreateFulfillmentCommand : ICommand<Empty>
{
    public string AllocationId { get; init; } = string.Empty;
}
