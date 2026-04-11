using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderDispatchedDomainEventHandler : INotificationHandler<OrderDispatchedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderDispatchedDomainEventHandler> _logger;

    public OrderDispatchedDomainEventHandler(IBus bus, ILogger<OrderDispatchedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderDispatchedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderDispatchedIntegrationEvent
        {
            Id = notification.OrderId,
            OrderNumber = notification.OrderNumber,
            CustomerId = notification.CustomerId,
            DispatchedAt = notification.OccurredOn,
            Items = notification.Lines
                .Select(l => new OrderItemModel
                {
                    Id = l.OrderItemId,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity
                })
                .ToList()
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderDispatchedIntegrationEvent),
            integrationEvent.Id,
            integrationEvent.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
