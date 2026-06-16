using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Domain.Returns.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Returns.Handlers;

public sealed class ReturnApprovedDomainEventHandler
    : INotificationHandler<ReturnApprovedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<ReturnApprovedDomainEventHandler> _logger;

    public ReturnApprovedDomainEventHandler(
        IBus bus,
        ILogger<ReturnApprovedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(
        ReturnApprovedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var @return = notification.Return;

        if (@return.Type == ReturnType.Immediate)
        {
            var integrationEvent = new ProcessImmediateReturnIntegrationEvent
            {
                ReturnId = @return.Id!
            };

            _logger.LogDebug(
                "Publishing integration event {EventName} for ReturnId={ReturnId}",
                nameof(ProcessImmediateReturnIntegrationEvent),
                integrationEvent.ReturnId);

            await _bus.Publish(integrationEvent);
        }
        else
        {
            _logger.LogDebug(
                "No integration event configured for return type {ReturnType}",
                @return.Type);
        }
    }
}
