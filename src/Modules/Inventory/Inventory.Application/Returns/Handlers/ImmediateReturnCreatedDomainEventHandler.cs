using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Inventory.Domain.Returns.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Returns.Handlers;

public class ImmediateReturnCreatedDomainEventHandler
    : INotificationHandler<ImmediateReturnCreatedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<ImmediateReturnCreatedDomainEventHandler> _logger;

    public ImmediateReturnCreatedDomainEventHandler(
        IBus bus,
        ILogger<ImmediateReturnCreatedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(
        ImmediateReturnCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new ImmediateReturnCreatedIntegrationEvent
        {
            ReturnId = notification.ImmediateReturn.Id!,
            OrderId = notification.ImmediateReturn.OrderId,
            AllocationId = notification.ImmediateReturn.AllocationId
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for ReturnId={ReturnId} OrderId={OrderId} AllocationId={AllocationId}",
            nameof(ImmediateReturnCreatedIntegrationEvent),
            integrationEvent.ReturnId,
            integrationEvent.OrderId,
            integrationEvent.AllocationId);

        await _bus.Publish(integrationEvent);
    }
}
