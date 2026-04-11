using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderRefusedDomainEventHandler : INotificationHandler<OrderRefusedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderRefusedDomainEventHandler> _logger;

    public OrderRefusedDomainEventHandler(IBus bus, ILogger<OrderRefusedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderRefusedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderRefusedIntegrationEvent
        {
            OrderId = notification.OrderId,
            OrderNumber = notification.OrderNumber,
            CustomerId = notification.CustomerId,
            RefusedAt = notification.OccurredOn
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderRefusedIntegrationEvent),
            integrationEvent.OrderId,
            integrationEvent.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
