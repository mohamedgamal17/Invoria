using Invoria.Inventory.Contracts.Events;
using Invoria.Inventory.Domain.Fulfillments.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Fulfillments.Handlers;

public sealed class FulfillmentCreatedDomainEventHandler : INotificationHandler<FulfillmentCreatedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<FulfillmentCreatedDomainEventHandler> _logger;

    public FulfillmentCreatedDomainEventHandler(
        IBus bus,
        ILogger<FulfillmentCreatedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(FulfillmentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new FulfillmentCreatedIntegrationEvent
        {
            FulfillmentId = notification.FulfillmentId,
            OrderId = notification.OrderId,
            AllocationId = notification.AllocationId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for FulfillmentId={FulfillmentId} OrderId={OrderId} AllocationId={AllocationId}",
            nameof(FulfillmentCreatedIntegrationEvent),
            integrationEvent.FulfillmentId,
            integrationEvent.OrderId,
            integrationEvent.AllocationId);

        await _bus.Publish(integrationEvent);
    }
}
