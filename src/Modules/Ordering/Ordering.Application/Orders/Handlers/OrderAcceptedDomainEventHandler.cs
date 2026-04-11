using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderAcceptedDomainEventHandler : INotificationHandler<OrderAcceptedDomainEvent>
{
    private readonly IBus _bus;

    public OrderAcceptedDomainEventHandler(IBus bus)
    {
        _bus = bus;
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

        await _bus.Publish(integrationEvent);
    }
}
