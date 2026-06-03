using Invoria.BuildingBlocks.Domain.Events;
using Invoria.Ordering.Application.Orders.Extensions;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Domain.Orders;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderEntityUpdatedDomainEventHandler : INotificationHandler<EntityUpdatedDomainEvent<Order, string>>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderEntityUpdatedDomainEventHandler> _logger;

    public OrderEntityUpdatedDomainEventHandler(IBus bus, ILogger<OrderEntityUpdatedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(EntityUpdatedDomainEvent<Order, string> notification, CancellationToken cancellationToken)
    {
        var order = notification.Entity;
        var integrationEvent = new OrderUpdatedIntegrationEvent
        {
            OccurredOn = notification.OccurredOn,
            Order = order.ToOrderModel()
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderUpdatedIntegrationEvent),
            integrationEvent.Order.Id,
            integrationEvent.Order.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
