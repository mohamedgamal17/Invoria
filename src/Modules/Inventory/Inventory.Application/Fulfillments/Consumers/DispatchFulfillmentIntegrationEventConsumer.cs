using Invoria.Inventory.Application.Fulfillments.Commands.DispatchFulfillment;
using Invoria.Inventory.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Fulfillments.Consumers;

public sealed class DispatchFulfillmentIntegrationEventConsumer
    : IHandleMessages<DispatchFulfillmentIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<DispatchFulfillmentIntegrationEventConsumer> _logger;

    public DispatchFulfillmentIntegrationEventConsumer(
        IMediator mediator,
        ILogger<DispatchFulfillmentIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(DispatchFulfillmentIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for FulfillmentId={FulfillmentId}",
            nameof(DispatchFulfillmentIntegrationEvent),
            message.FulfillmentId);

        return _mediator.Send(DispatchFulfillmentCommand.FromEvent(message), CancellationToken.None);
    }
}
