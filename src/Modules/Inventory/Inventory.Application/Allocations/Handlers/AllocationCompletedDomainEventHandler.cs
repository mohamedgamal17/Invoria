using Invoria.Inventory.Contracts.Events;
using Invoria.Inventory.Domain.Allocations.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Allocations.Handlers;

public sealed class AllocationCompletedDomainEventHandler : INotificationHandler<AllocationCompletedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<AllocationCompletedDomainEventHandler> _logger;

    public AllocationCompletedDomainEventHandler(
        IBus bus,
        ILogger<AllocationCompletedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(AllocationCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new AllocationSucceededIntegrationEvent
        {
            AllocationId = notification.AllocationId,
            OrderId = notification.OrderId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for AllocationId={AllocationId} OrderId={OrderId}",
            nameof(AllocationSucceededIntegrationEvent),
            integrationEvent.AllocationId,
            integrationEvent.OrderId);

        await _bus.Publish(integrationEvent);
    }
}
