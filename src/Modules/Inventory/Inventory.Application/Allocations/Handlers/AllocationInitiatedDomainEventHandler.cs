using Invoria.Inventory.Contracts.Events;
using Invoria.Inventory.Domain.Allocations.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Allocations.Handlers;

public sealed class AllocationInitiatedDomainEventHandler : INotificationHandler<AllocationInitiatedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<AllocationInitiatedDomainEventHandler> _logger;

    public AllocationInitiatedDomainEventHandler(
        IBus bus,
        ILogger<AllocationInitiatedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(AllocationInitiatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new RequestAllocationIntegrationEvent
        {
            AllocationId = notification.AllocationId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for AllocationId={AllocationId}",
            nameof(RequestAllocationIntegrationEvent),
            integrationEvent.AllocationId);

        await _bus.Publish(integrationEvent);
    }
}
