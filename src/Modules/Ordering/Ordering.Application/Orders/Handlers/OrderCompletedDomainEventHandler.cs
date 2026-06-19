using Invoria.Ordering.Application.Orders.Extensions;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderCompletedDomainEventHandler : INotificationHandler<OrderCompletedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderCompletedDomainEventHandler> _logger;

    public OrderCompletedDomainEventHandler(
        IBus bus,
        ILogger<OrderCompletedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = notification.Order.ToOrderCompletedIntegrationEvent(notification.OccurredOn);

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId}",
            nameof(OrderCompletedIntegrationEvent),
            integrationEvent.OrderId);
        await _bus.Publish(integrationEvent);
    }
}
