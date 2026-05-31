using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Inventory.Application.Fulfillments.Commands.DispatchFulfillment;

public sealed class DispatchFulfillmentCommand : ICommand<Empty>
{
    public string FulfillmentId { get; init; } = string.Empty;
}
