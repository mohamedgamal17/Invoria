using Invoria.Inventory.Contracts.Events;
using Invoria.Inventory.Domain.Fulfillments.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Fulfillments.Handlers;

public sealed class RequestDispatchDomainEventHandler : INotificationHandler<RequestDispatchDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<RequestDispatchDomainEventHandler> _logger;

    public RequestDispatchDomainEventHandler(
        IBus bus,
        ILogger<RequestDispatchDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(RequestDispatchDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new DispatchFulfillmentIntegrationEvent
        {
            FulfillmentId = notification.FulfillmentId,
            OrderId = notification.OrderId,
            AllocationId = notification.AllocationId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for FulfillmentId={FulfillmentId} OrderId={OrderId} AllocationId={AllocationId}",
            nameof(DispatchFulfillmentIntegrationEvent),
            integrationEvent.FulfillmentId,
            integrationEvent.OrderId,
            integrationEvent.AllocationId);

        await _bus.Publish(integrationEvent);
    }
}
