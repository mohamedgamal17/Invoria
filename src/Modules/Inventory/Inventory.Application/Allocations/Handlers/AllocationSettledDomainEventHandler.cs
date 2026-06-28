using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Domain.Allocations.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Allocations.Handlers;

public sealed class AllocationSettledDomainEventHandler : INotificationHandler<AllocationSettledDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<AllocationSettledDomainEventHandler> _logger;

    public AllocationSettledDomainEventHandler(
        IBus bus,
        ILogger<AllocationSettledDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(AllocationSettledDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new AllocationSettledIntegrationEvent
        {
            AllocationId = notification.Allocation.Id!,
            OrderId = notification.Allocation.OrderId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for AllocationId={AllocationId} OrderId={OrderId}",
            nameof(AllocationSettledIntegrationEvent),
            integrationEvent.AllocationId,
            integrationEvent.OrderId);

        await _bus.Publish(integrationEvent);
    }
}
