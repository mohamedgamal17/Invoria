using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
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
        var integrationEvent = new AllocateOrderIntegrationEvent
        {
            Id = notification.OrderId,
            OrderNumber = notification.OrderNumber,
            CustomerId = notification.CustomerId,
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
            nameof(AllocateOrderIntegrationEvent),
            integrationEvent.Id,
            integrationEvent.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
