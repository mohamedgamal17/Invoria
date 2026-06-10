using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
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
        var order = notification.Order;

        if (order.ReturnItems.Count == 0 || string.IsNullOrEmpty(order.AllocationId))
        {
            return;
        }

        var lines = order.ReturnItems
            .Select(returnItem =>
            {
                var orderLine = order.Items.Single(i => i.Id == returnItem.OrderItemId);
                return new OrderReturnLineModel
                {
                    OrderItemId = returnItem.OrderItemId,
                    ProductId = orderLine.ProductId,
                    Quantity = returnItem.Quantity
                };
            })
            .ToList();

        var integrationEvent = new OrderReturnRequestedIntegrationEvent
        {
            OrderId = order.Id,
            AllocationId = order.AllocationId,
            Lines = lines
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} AllocationId={AllocationId}",
            nameof(OrderReturnRequestedIntegrationEvent),
            integrationEvent.OrderId,
            integrationEvent.AllocationId);
        await _bus.Publish(integrationEvent);
    }
}
