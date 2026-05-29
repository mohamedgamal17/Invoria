using Invoria.Inventory.Contracts.Events;
using Invoria.Inventory.Domain.Fulfillments.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Fulfillments.Handlers;

public sealed class FulfillmentCompletedDomainEventHandler : INotificationHandler<FulfillmentCompletedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<FulfillmentCompletedDomainEventHandler> _logger;

    public FulfillmentCompletedDomainEventHandler(
        IBus bus,
        ILogger<FulfillmentCompletedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(FulfillmentCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new FulfillmentCompletedIntegrationEvent
        {
            FulfillmentId = notification.FulfillmentId,
            OrderId = notification.OrderId,
            AllocationId = notification.AllocationId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for FulfillmentId={FulfillmentId} OrderId={OrderId} AllocationId={AllocationId}",
            nameof(FulfillmentCompletedIntegrationEvent),
            integrationEvent.FulfillmentId,
            integrationEvent.OrderId,
            integrationEvent.AllocationId);

        await _bus.Publish(integrationEvent);
    }
}
