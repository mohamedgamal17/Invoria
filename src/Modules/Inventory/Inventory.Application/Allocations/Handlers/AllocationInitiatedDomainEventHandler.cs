using Invoria.Inventory.Application.Allocations.Extensions;
using Invoria.Inventory.Contracts.Allocations.Events;
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
        var integrationEvent = new AllocationCreatedIntegrationEvent
        {
            Allocation = notification.Allocation.ToAllocationModel(),
            OccurredOn = DateTimeOffset.UtcNow
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for AllocationId={AllocationId} OrderId={OrderId}",
            nameof(AllocationCreatedIntegrationEvent),
            integrationEvent.Allocation.Id,
            integrationEvent.Allocation.OrderId);

        await _bus.Publish(integrationEvent);
    }
}
