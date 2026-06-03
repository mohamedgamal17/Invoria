using Invoria.Ordering.Application.Orders.Extensions;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderAcceptedDomainEventHandler : INotificationHandler<OrderAcceptedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderAcceptedDomainEventHandler> _logger;

    public OrderAcceptedDomainEventHandler(IBus bus, ILogger<OrderAcceptedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderAcceptedDomainEvent notification, CancellationToken cancellationToken)
    {
        var order = notification.Order;

        var integrationEvent = new OrderAcceptedIntegrationEvent
        {
            OccurredOn = notification.OccurredOn,
            Order = order.ToOrderModel()
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderAcceptedIntegrationEvent),
            integrationEvent.Order.Id,
            integrationEvent.Order.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
