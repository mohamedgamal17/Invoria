using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Events;

namespace Invoria.Inventory.Application.Fulfillments.Commands.DispatchFulfillment;

public sealed class DispatchFulfillmentCommand : ICommand<Empty>
{
    public string FulfillmentId { get; init; } = string.Empty;

    public static DispatchFulfillmentCommand FromEvent(DispatchFulfillmentIntegrationEvent message) =>
        new() { FulfillmentId = message.FulfillmentId };
}
