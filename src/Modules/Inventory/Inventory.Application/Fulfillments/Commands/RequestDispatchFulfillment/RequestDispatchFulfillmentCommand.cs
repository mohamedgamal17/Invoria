using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Inventory.Application.Fulfillments.Commands.RequestDispatchFulfillment;

public sealed class RequestDispatchFulfillmentCommand : ICommand<Empty>
{
    public string FulfillmentId { get; init; } = string.Empty;
}
