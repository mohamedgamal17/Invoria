using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Domain.Allocations.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Allocations.Handlers;

public sealed class AllocationReleasedDomainEventHandler : INotificationHandler<AllocationReleasedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<AllocationReleasedDomainEventHandler> _logger;

    public AllocationReleasedDomainEventHandler(
        IBus bus,
        ILogger<AllocationReleasedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(AllocationReleasedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new AllocationReleasedIntegrationEvent
        {
            AllocationId = notification.Allocation.Id!,
            OrderId = notification.Allocation.OrderId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for AllocationId={AllocationId} OrderId={OrderId}",
            nameof(AllocationReleasedIntegrationEvent),
            integrationEvent.AllocationId,
            integrationEvent.OrderId);

        await _bus.Publish(integrationEvent);
    }
}
