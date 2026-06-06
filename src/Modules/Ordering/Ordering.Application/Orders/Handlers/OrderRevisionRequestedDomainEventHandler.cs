using Invoria.Ordering.Application.Orders.Extensions;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderRevisionRequestedDomainEventHandler
    : INotificationHandler<OrderRevisionRequestedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderRevisionRequestedDomainEventHandler> _logger;

    public OrderRevisionRequestedDomainEventHandler(
        IBus bus,
        ILogger<OrderRevisionRequestedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderRevisionRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        var order = notification.Order;

        var integrationEvent = new OrderRevisionRequestedIntegrationEvent
        {
            OccurredOn = notification.OccurredOn,
            Order = order.ToOrderModel(),
            AllocationId = order.AllocationId!
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} AllocationId={AllocationId}",
            nameof(OrderRevisionRequestedIntegrationEvent),
            integrationEvent.Order.Id,
            integrationEvent.AllocationId);
        await _bus.Publish(integrationEvent);
    }
}
