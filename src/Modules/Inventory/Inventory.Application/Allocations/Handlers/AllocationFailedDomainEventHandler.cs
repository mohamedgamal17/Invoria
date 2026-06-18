using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Domain.Allocations.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Allocations.Handlers;

public sealed class AllocationFailedDomainEventHandler : INotificationHandler<AllocationFailedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<AllocationFailedDomainEventHandler> _logger;

    public AllocationFailedDomainEventHandler(
        IBus bus,
        ILogger<AllocationFailedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(AllocationFailedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new AllocationFailedIntegrationEvent
        {
            AllocationId = notification.Allocation.Id!,
            OrderId = notification.Allocation.OrderId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for AllocationId={AllocationId} OrderId={OrderId}",
            nameof(AllocationFailedIntegrationEvent),
            integrationEvent.AllocationId,
            integrationEvent.OrderId);

        await _bus.Publish(integrationEvent);
    }
}
